namespace LCDSimulator
{
    [Flags]
    public enum DisplayComponents
    {
        None = 0,
        Display = 0b1,
        Cursor = 0b10,
        Blink = 0b100
    }

    public enum DisplayFunction
    {
        OneLine5x8,
        OneLine5x10,
        TwoLine5x8
    }

    public enum DisplayInstruction : byte
    {
        Unknown = 0,
        ClearDisplay = 0b1,
        ReturnHome = 0b10,
        EntryModeSet = 0b100,
        DisplayControl = 0b1000,
        Shift = 0b10000,
        FunctionSet = 0b100000,
        SetCGRAMAddress = 0b1000000,
        SetDDRAMAddress = 0b10000000
    }

    public class DisplayController : IDisposable
    {
        public const byte CGRAMAddressMask = 0b111111;
        public const byte DDRAMAddressMask = 0b1111111;

        public const byte CGRAMCharacterCodeEnd = 0b10000;

        public const byte CharactersPerLine = 40;
        public const byte MaximumCharacterCount = CharactersPerLine * 2;

        public const byte SecondLineStartAddress = 64;
        public const byte MaximumDDRAMAddress = SecondLineStartAddress + CharactersPerLine - 1;

        public const byte BusyFlagBit = 0b10000000;

        public const byte BlankCharacter = 0x20; // ' '

        public const byte DotsPerCharacterWidth = 5;
        public const byte DotsPerCharacterHeight = 8;
        public const byte DotsInExtendedCharacterHeight = 3;

        // MPU Pins
        public bool RegisterSelect { get; set; } = false;
        public bool ReadWrite { get; set; } = false;
        public byte DataBus { get; set; } = 0;

        private bool _isPowered = false;
        public bool IsPowered
        {
            get => _isPowered;
            set
            {
                if (!value)
                {
                    Reset();
                }

                _isPowered = value;

                UpdateRenderedDots();
            }
        }

        public bool Backlight { get; set; }

        private bool _enable = false;
        public bool Enable
        {
            get => _enable;
            set
            {
                if (!_enable && value)
                {
                    EnableRisingEdge();
                }
                else if (_enable && !value)
                {
                    EnableFallingEdge();
                }
                _enable = value;
            }
        }

        // Rendered dots
        // In 1-line mode, a maximum of 40 characters are driven, even though DDRAM is 80 characters long
        public readonly bool[,] FirstLineDots = new bool[CharactersPerLine * DotsPerCharacterWidth, DotsPerCharacterHeight];
        public readonly bool[,] SecondLineDots = new bool[CharactersPerLine * DotsPerCharacterWidth, DotsPerCharacterHeight];

        public readonly byte[] FirstLineDDRAMAddresses = new byte[CharactersPerLine];
        public readonly byte[] SecondLineDDRAMAddresses = new byte[CharactersPerLine];

        // Internal State (Inaccessible on a real controller without going through MPU)
        private byte _addressCounter = 0;
        public byte AddressCounter
        {
            get => _addressCounter;
            private set
            {
                if (!AddressingCharacterGeneratorRAM)
                {
                    byte ddramAddress = (byte)(value & DDRAMAddressMask);
                    // Wrap around once end of character data is reached
                    if (ddramAddress == DDRAMAddressMask)
                    {
                        value = CurrentDDRAMAddressLimit;
                    }
                    else if (ddramAddress > CurrentDDRAMAddressLimit)
                    {
                        value = 0;
                    }
                    // Move onto next/previous line
                    else if (TwoLineMode && ddramAddress is >= CharactersPerLine and < SecondLineStartAddress)
                    {
                        value = (byte)(IncrementOnReadWrite ? SecondLineStartAddress : CharactersPerLine - 1);
                    }
                }
                _addressCounter = value;
            }
        }

        public bool BusyFlag { get; private set; } = false;
        public byte InstructionRegister { get; private set; } = 0;
        public byte DataRegister { get; private set; } = 0;

        public DDRAM DisplayDataRAM { get; } = new();
        public byte[] CharacterGeneratorRAM { get; } = new byte[64];
        public byte[] CharacterGeneratorROM { get; } = new byte[4096];

        // Simulator Specific Properties
        public bool TwoLineMode => CurrentDisplayFunction == DisplayFunction.TwoLine5x8;
        public bool ExtendedCharacterHeight => CurrentDisplayFunction == DisplayFunction.OneLine5x10;
        public byte CurrentDDRAMAddressLimit => TwoLineMode ? MaximumDDRAMAddress : (byte)(MaximumCharacterCount - 1);

        public byte DataReadBuffer { get; private set; } = 0;

        public bool AddressingCharacterGeneratorRAM { get; private set; } = false;

        public DisplayComponents EnabledDisplayComponents { get; private set; } = DisplayComponents.None;
        public bool IncrementOnReadWrite { get; private set; } = true;
        public bool ShiftScreenOnWrite { get; private set; } = false;

        public bool FourBitMode { get; private set; } = false;
        public DisplayFunction CurrentDisplayFunction { get; private set; } = DisplayFunction.OneLine5x8;

        private sbyte _displayShiftAmount = 0;
        public sbyte DisplayShiftAmount
        {
            get => _displayShiftAmount;
            private set
            {
                sbyte limit = (sbyte)(TwoLineMode ? CharactersPerLine : MaximumCharacterCount);
                if (value <= -limit || value >= limit)
                {
                    value = 0;
                }
                _displayShiftAmount = value;
            }
        }

        public bool AwaitingSecondInstruction { get; private set; } = false;
        public bool PendingLowerAddressRead { get; private set; } = false;

        public readonly byte[] BlinkPixels5x8 = Characters.GetImageData(Characters.GetImagePath(Characters.Blink5x8));
        public readonly byte[] BlinkPixels5x11 = Characters.GetImageData(Characters.GetImagePath(Characters.Blink5x11));
        public readonly byte[] CursorPixels5x8 = Characters.GetImageData(Characters.GetImagePath(Characters.Cursor5x8));
        public readonly byte[] CursorPixels5x11 = Characters.GetImageData(Characters.GetImagePath(Characters.Cursor5x11));

        private readonly CursorBlink blinkController = new();

        public DisplayController()
        {
            Characters.LoadPixelData(CharacterGeneratorROM);

            Reset();
        }

        ~DisplayController()
        {
            Dispose();
        }

        public void Dispose()
        {
            blinkController.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            ClearDisplay();

            // Set CGRAM to default pattern (alternating horizontal stripes)
            for (int i = 0; i < CharacterGeneratorRAM.Length; i++)
            {
                CharacterGeneratorRAM[i] = (byte)(i % 2 == 0 ? byte.MaxValue : 0);
            }

            FourBitMode = false;
            AwaitingSecondInstruction = false;
            PendingLowerAddressRead = false;

            CurrentDisplayFunction = DisplayFunction.OneLine5x8;

            EnabledDisplayComponents = DisplayComponents.None;

            IncrementOnReadWrite = true;
            ShiftScreenOnWrite = false;

            DataReadBuffer = BlankCharacter;

            UpdateRenderedDots();
        }

        private void EnableRisingEdge()
        {
            // When reading, pins are set up once Enable pin goes high
            if (!IsPowered || !ReadWrite)
            {
                return;
            }

            if (RegisterSelect)
            {
                // Read RAM data
                ProcessDataRead();

                IncrementAddressCounter();

                PendingLowerAddressRead = false;
            }
            else
            {
                // Read busy flag and current address
                byte readValue = (byte)(AddressCounter & DDRAMAddressMask);
                if (BusyFlag)
                {
                    readValue &= BusyFlagBit;
                }

                if (FourBitMode)
                {
                    DataBus = (byte)((PendingLowerAddressRead ? readValue << 4 : readValue & 0b11110000) | 0b1111);
                    PendingLowerAddressRead = !PendingLowerAddressRead;
                }
                else
                {
                    DataBus = readValue;
                }
            }
        }

        private void EnableFallingEdge()
        {
            // Operations are run once Enable pin goes low
            if (!IsPowered || (ReadWrite && !RegisterSelect))
            {
                return;
            }

            PendingLowerAddressRead = false;

            BusyFlag = true;

            try
            {
                if (!ReadWrite)
                {
                    if (RegisterSelect)
                    {
                        if (FourBitMode)
                        {
                            // In 4-bit mode, data register is updated one half at a time
                            if (AwaitingSecondInstruction)
                            {
                                DataRegister = (byte)((DataRegister & 0b11110000) | (DataBus >> 4));
                                ProcessDataWrite();
                                AwaitingSecondInstruction = false;
                            }
                            else
                            {
                                DataRegister = (byte)((DataRegister & 0b1111) | (DataBus & 0b11110000));
                                AwaitingSecondInstruction = true;
                            }
                        }
                        else
                        {
                            DataRegister = DataBus;
                            ProcessDataWrite();
                        }

                        if (!AwaitingSecondInstruction)
                        {
                            IncrementAddressCounter();
                        }
                    }
                    else
                    {
                        if (FourBitMode)
                        {
                            // In 4-bit mode, instruction register is updated one half at a time
                            if (AwaitingSecondInstruction)
                            {
                                InstructionRegister = (byte)((InstructionRegister & 0b11110000) | (DataBus >> 4));
                                ProcessInstructionRegister();
                                AwaitingSecondInstruction = false;
                            }
                            else
                            {
                                InstructionRegister = (byte)((InstructionRegister & 0b1111) | (DataBus & 0b11110000));
                                AwaitingSecondInstruction = true;
                            }
                        }
                        else
                        {
                            InstructionRegister = DataBus;
                            ProcessInstructionRegister();
                        }
                    }
                }

                if ((!RegisterSelect || ReadWrite) && !AwaitingSecondInstruction)
                {
                    // This simulates the behaviour of real controllers when invalidly performing a
                    // RAM read operation directly after a RAM write.
                    DataReadBuffer = AddressingCharacterGeneratorRAM
                        ? CharacterGeneratorRAM[AddressCounter & CGRAMAddressMask]
                        : DisplayDataRAM[AddressCounter & DDRAMAddressMask, TwoLineMode];
                }
            }
            finally
            {
                BusyFlag = false;
            }

            UpdateRenderedDots();
        }

        public void UpdateRenderedDots()
        {
            if (TwoLineMode)
            {
                for (int i = 0; i < CharactersPerLine; i++)
                {
                    int ddramAddress = Mod(i - DisplayShiftAmount, CharactersPerLine);
                    FirstLineDDRAMAddresses[i] = (byte)ddramAddress;
                    RenderCharacter(ddramAddress, i, DisplayDataRAM[ddramAddress, true], false);
                }
                for (int i = 0; i < CharactersPerLine; i++)
                {
                    int ddramAddress = SecondLineStartAddress + Mod(i - DisplayShiftAmount, CharactersPerLine);
                    SecondLineDDRAMAddresses[i] = (byte)ddramAddress;
                    RenderCharacter(ddramAddress, i, DisplayDataRAM[ddramAddress, true], true);
                }
            }
            else
            {
                // Blank out second line before rendering in 1-line mode
                for (int x = 0; x < SecondLineDots.GetLength(0); x++)
                {
                    for (int y = 0; y < SecondLineDots.GetLength(1); y++)
                    {
                        SecondLineDots[x, y] = false;
                    }
                }

                for (int i = 0; i < CharactersPerLine; i++)
                {
                    int ddramAddress = Mod(i - DisplayShiftAmount, CharactersPerLine);
                    FirstLineDDRAMAddresses[i] = (byte)ddramAddress;
                    RenderCharacter(ddramAddress, i, DisplayDataRAM[ddramAddress, false], false);
                }
            }
        }

        private void ProcessDataRead()
        {
            if (FourBitMode)
            {
                DataBus = (byte)((AwaitingSecondInstruction ? DataReadBuffer << 4 : DataReadBuffer & 0b11110000) | 0b1111);
                AwaitingSecondInstruction = !AwaitingSecondInstruction;
            }
            else
            {
                DataBus = DataReadBuffer;
            }
        }

        private void ProcessDataWrite()
        {
            if (AddressingCharacterGeneratorRAM)
            {
                CharacterGeneratorRAM[AddressCounter & CGRAMAddressMask] = DataRegister;
            }
            else
            {
                DisplayDataRAM[AddressCounter & DDRAMAddressMask, TwoLineMode] = DataRegister;
            }
        }

        private void ProcessInstructionRegister()
        {
            switch (DecodeInstruction(InstructionRegister))
            {
                case DisplayInstruction.ClearDisplay:
                    ClearDisplay();
                    break;
                case DisplayInstruction.ReturnHome:
                    ReturnHome();
                    break;
                case DisplayInstruction.EntryModeSet:
                    EntryModeSet();
                    break;
                case DisplayInstruction.DisplayControl:
                    DisplayControl();
                    break;
                case DisplayInstruction.Shift:
                    Shift();
                    break;
                case DisplayInstruction.FunctionSet:
                    FunctionSet();
                    break;
                case DisplayInstruction.SetCGRAMAddress:
                    SetCGRAMAddress();
                    break;
                case DisplayInstruction.SetDDRAMAddress:
                    SetDDRAMAddress();
                    break;
                case DisplayInstruction.Unknown:
                default:
                    break;
            }
        }

        private void IncrementAddressCounter()
        {
            if (IncrementOnReadWrite)
            {
                AddressCounter++;
            }
            else
            {
                AddressCounter--;
            }

            if (ShiftScreenOnWrite && !ReadWrite && !AddressingCharacterGeneratorRAM)
            {
                if (IncrementOnReadWrite)
                {
                    DisplayShiftAmount--;
                }
                else
                {
                    DisplayShiftAmount++;
                }
            }
        }

        private void RenderCharacter(int ddramAddress, int indexOnLine, byte characterCode, bool secondLine)
        {
            if ((EnabledDisplayComponents & DisplayComponents.Display) == 0)
            {
                // Display is disabled, render blank character and nothing else
                RenderFont(indexOnLine, BlankCharacter, secondLine);
                return;
            }

            RenderFont(indexOnLine, characterCode, secondLine);

            if ((AddressCounter & DDRAMAddressMask) == (ddramAddress & DDRAMAddressMask))
            {
                // Render cursor onto this character
                if ((EnabledDisplayComponents & DisplayComponents.Cursor) != 0)
                {
                    RenderPixels(ExtendedCharacterHeight ? CursorPixels5x11 : CursorPixels5x8,
                        indexOnLine, secondLine, true);
                }

                if (blinkController.Blink && (EnabledDisplayComponents & DisplayComponents.Blink) != 0)
                {
                    RenderPixels(ExtendedCharacterHeight ? BlinkPixels5x11 : BlinkPixels5x8,
                        indexOnLine, secondLine, true);
                }
            }
        }

        private void RenderFont(int indexOnLine, byte characterCode, bool secondLine)
        {
            if (secondLine && !TwoLineMode)
            {
                throw new ArgumentException("secondLine cannot be true when not in two line mode.");
            }

            Span<byte> pixelDataSource;
            if (characterCode < CGRAMCharacterCodeEnd)
            {
                // This is a custom character - get character data from CGRAM instead of CGROM
                pixelDataSource = ExtendedCharacterHeight
                    ? CharacterGeneratorRAM.AsSpan((characterCode & 0b110) << 3, 11)
                    : CharacterGeneratorRAM.AsSpan((characterCode & 0b111) << 3, 8);
            }
            else
            {
                pixelDataSource = CharacterGeneratorROM.AsSpan(characterCode << 4, Characters.MaximumImageHeight);
            }

            RenderPixels(pixelDataSource, indexOnLine, secondLine, false);
        }

        private void RenderPixels(Span<byte> pixelDataSource, int indexOnLine, bool secondLine, bool combineWithExisting)
        {
            if (secondLine && !TwoLineMode)
            {
                throw new ArgumentException("secondLine cannot be true when not in two line mode.");
            }
            if (pixelDataSource.Length < DotsPerCharacterHeight ||
                (ExtendedCharacterHeight && pixelDataSource.Length < DotsPerCharacterHeight + DotsInExtendedCharacterHeight))
            {
                throw new ArgumentException("pixelDataSource does not contain enough rows for current display mode.");
            }

            int startX = indexOnLine * DotsPerCharacterWidth;
            bool[,] targetArray = secondLine ? SecondLineDots : FirstLineDots;
            for (int y = 0; y < DotsPerCharacterHeight; y++)
            {
                byte row = pixelDataSource[y];
                // x is iterated in reverse so that lower bits start on the right
                for (int x = DotsPerCharacterWidth - 1; x >= 0; x--, row >>= 1)
                {
                    targetArray[startX + x, y] = (row & 0b1) != 0
                        // Combining with existing always keeps lit dots, but overwrites unlit dots
                        || (combineWithExisting && targetArray[startX + x, y]);
                }
            }

            if (ExtendedCharacterHeight)
            {
                // Use the second line to display the remaining bottom dots in 5x10 mode
                // (5x10 as named in the spec is actually 5x11)
                for (int y = 0; y < DotsInExtendedCharacterHeight; y++)
                {
                    byte row = pixelDataSource[DotsPerCharacterHeight + y];
                    // x is iterated in reverse so that lower bits start on the right
                    for (int x = DotsPerCharacterWidth - 1; x >= 0; x--, row >>= 1)
                    {
                        SecondLineDots[startX + x, y] = (row & 0b1) != 0
                            // Combining with existing always keeps lit dots, but overwrites unlit dots
                            || (combineWithExisting && SecondLineDots[startX + x, y]);
                    }
                }
                // The remaining dots beneath the extended length in 5x10 mode should be blank
                for (int y = DotsInExtendedCharacterHeight; y < DotsPerCharacterHeight; y++)
                {
                    for (int x = 0; x < DotsPerCharacterWidth; x++)
                    {
                        SecondLineDots[startX + x, y] = false;
                    }
                }
            }
        }

        private void ClearDisplay()
        {
            DisplayDataRAM.Reset();

            ReturnHome();

            IncrementOnReadWrite = true;
        }

        private void ReturnHome()
        {
            AddressingCharacterGeneratorRAM = false;
            AddressCounter = 0;

            DisplayShiftAmount = 0;
        }

        private void EntryModeSet()
        {
            IncrementOnReadWrite = (InstructionRegister & 0b10) != 0;
            ShiftScreenOnWrite = (InstructionRegister & 0b1) != 0;
        }

        private void DisplayControl()
        {
            EnabledDisplayComponents = DisplayComponents.None;
            if ((InstructionRegister & 0b100) != 0)
            {
                EnabledDisplayComponents |= DisplayComponents.Display;
            }
            if ((InstructionRegister & 0b10) != 0)
            {
                EnabledDisplayComponents |= DisplayComponents.Cursor;
            }
            if ((InstructionRegister & 0b1) != 0)
            {
                EnabledDisplayComponents |= DisplayComponents.Blink;
            }
        }

        private void Shift()
        {
            AddressingCharacterGeneratorRAM = false;

            if ((InstructionRegister & 0b1000) != 0)
            {
                // Shift screen
                if ((InstructionRegister & 0b100) != 0)
                {
                    DisplayShiftAmount++;
                }
                else
                {
                    DisplayShiftAmount--;
                }
            }
            else
            {
                // Shift cursor
                if ((InstructionRegister & 0b100) != 0)
                {
                    AddressCounter++;
                }
                else
                {
                    AddressCounter--;
                }
            }
        }

        private void FunctionSet()
        {
            FourBitMode = (InstructionRegister & 0b10000) == 0;

            CurrentDisplayFunction = (InstructionRegister & 0b1000) != 0
                ? DisplayFunction.TwoLine5x8
                : (InstructionRegister & 0b100) != 0
                    ? DisplayFunction.OneLine5x10
                    : DisplayFunction.OneLine5x8;

            // This will ensure that address counter is still a valid value
            // when switching between 1 and 2 line mode.
            AddressCounter = _addressCounter;
        }

        private void SetCGRAMAddress()
        {
            AddressingCharacterGeneratorRAM = true;
            AddressCounter = InstructionRegister;
        }

        private void SetDDRAMAddress()
        {
            AddressingCharacterGeneratorRAM = false;
            AddressCounter = InstructionRegister;
        }

        private static DisplayInstruction DecodeInstruction(byte instruction)
        {
            return Enum.GetValues<DisplayInstruction>().OrderDescending()
                .FirstOrDefault(i => (instruction & (byte)i) != 0);
        }

        internal static int Mod(int a, int b)
        {
            // Needed because C#'s % operator finds the remainder, not modulo
            return ((a % b) + b) % b;
        }
    }
}
