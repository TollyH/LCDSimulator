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

    public class DisplayController
    {
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
                    if ((value & 0b1111111) == 0b1111111)
                    {
                        value = 79;
                    }
                    else if ((value & 0b1111111) >= 80)
                    {
                        value = 0;
                    }
                    // Move onto next/previous line
                    if (value is >= 40 and < 64)
                    {
                        value = (byte)(IncrementOnReadWrite ? 64 : 39);
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
        public bool AddressingCharacterGeneratorRAM { get; private set; } = false;

        public DisplayComponents EnabledDisplayComponents { get; private set; } = DisplayComponents.None;
        public bool IncrementOnReadWrite { get; private set; } = true;
        public bool ShiftScreenOnReadWrite { get; private set; } = false;

        public bool FourBitMode { get; private set; } = false;
        public DisplayFunction CurrentDisplayFunction { get; private set; } = DisplayFunction.OneLine5x8;

        public sbyte DisplayShiftAmount { get; private set; } = 0;
    }
}
