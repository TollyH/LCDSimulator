﻿namespace LCDSimulator
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

    public class DisplayController
    {
        public const byte CGRAMAddressMask = 0b111111;
        public const byte DDRAMAddressMask = 0b1111111;

        public const byte CharactersPerLine = 40;
        public const byte MaximumCharacterCount = CharactersPerLine * 2;

        public const byte SecondLineStartAddress = 64;
        public const byte MaximumDDRAMAddress = SecondLineStartAddress + CharactersPerLine - 1;

        public const byte BusyFlagBit = 0b10000000;

        public const byte BlankCharacter = 0x20; // ' '

        // MPU Pins
        public bool RegisterSelect { get; set; } = false;
        public bool ReadWrite { get; set; } = false;
        public byte DataBus { get; set; } = 0;

        // Internal State (Inaccessible on a real controller without going through MPU)
        private byte _addressCounter = 0;
        public byte AddressCounter
        {
            get => _addressCounter;
            private set
            {
                if (!AddressingCharacterGeneratorRAM)
                {
                    // Wrap around once end of character data is reached
                    if ((value & DDRAMAddressMask) == DDRAMAddressMask)
                    {
                        value = CurrentDDRAMAddressLimit;
                    }
                    else if ((value & DDRAMAddressMask) > CurrentDDRAMAddressLimit)
                    {
                        value = 0;
                    }
                    // Move onto next/previous line
                    if (TwoLineMode && value is >= CharactersPerLine and < SecondLineStartAddress)
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
        public byte CurrentDDRAMAddressLimit => TwoLineMode ? MaximumDDRAMAddress : (byte)(MaximumCharacterCount - 1);

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

        public DisplayController()
        {
            Reset();
        }

        public void Reset()
        {
            ClearDisplay();

            FourBitMode = false;
            CurrentDisplayFunction = DisplayFunction.OneLine5x8;

            EnabledDisplayComponents = DisplayComponents.None;

            IncrementOnReadWrite = true;
            ShiftScreenOnWrite = false;
        }

        public void CycleEnablePin()
        {
            // TODO: 4-bit mode (test effect of changing RS/RW mid-operation)

            if (!RegisterSelect && ReadWrite)
            {
                // Read busy flag and current address - bypasses registers
                DataBus = (byte)(AddressCounter & DDRAMAddressMask);
                if (BusyFlag)
                {
                    DataBus &= BusyFlagBit;
                }
                return;
            }

            BusyFlag = true;

            try
            {
                if (RegisterSelect)
                {
                    DataRegister = DataBus;
                    ProcessDataRegister();

                    IncrementAddressCounter();

                    if (ReadWrite)
                    {
                        DataBus = DataRegister;
                    }
                }
                else
                {
                    InstructionRegister = DataBus;
                    ProcessInstructionRegister();
                }
            }
            finally
            {
                BusyFlag = false;
            }
        }

        private void ProcessDataRegister()
        {
            if (ReadWrite)
            {
                // Read data from RAM
                DataRegister = AddressingCharacterGeneratorRAM
                    ? CharacterGeneratorRAM[AddressCounter & CGRAMAddressMask]
                    : DisplayDataRAM[AddressCounter & DDRAMAddressMask, TwoLineMode];
            }
            else
            {
                // Write data to RAM
                if (AddressingCharacterGeneratorRAM)
                {
                    CharacterGeneratorRAM[AddressCounter & CGRAMAddressMask] = DataRegister;
                }
                else
                {
                    DisplayDataRAM[AddressCounter & DDRAMAddressMask, TwoLineMode] = DataRegister;
                }
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

        private void ClearDisplay()
        {
            for (int i = 0; i < CharactersPerLine; i++)
            {
                DisplayDataRAM[i, TwoLineMode] = BlankCharacter;
            }
            for (int i = SecondLineStartAddress; i <= MaximumDDRAMAddress; i++)
            {
                DisplayDataRAM[i, TwoLineMode] = BlankCharacter;
            }

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

            CurrentDisplayFunction = (InstructionRegister & 0b100) != 0
                ? DisplayFunction.OneLine5x10
                : (InstructionRegister & 0b1000) != 0
                    ? DisplayFunction.TwoLine5x8
                    : DisplayFunction.OneLine5x8;
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
    }
}