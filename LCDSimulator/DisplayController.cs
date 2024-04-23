namespace LCDSimulator
{
    public class DisplayController
    {
        // MPU Pins
        public bool RegisterSelect { get; set; }
        public bool ReadWrite { get; set; }
        public byte DataBus { get; set; }

        // Internal State (Inaccessible on a real controller without going through MPU)
        public byte AddressCounter { get; private set; }
        public bool BusyFlag { get; private set; }
        public byte InstructionRegister { get; private set; }
        public byte DataRegister { get; private set; }

        public byte[] DisplayDataRAM { get; } = new byte[80];
        public byte[] CharacterGeneratorRAM { get; } = new byte[64];
        public byte[] CharacterGeneratorROM { get; } = new byte[4096];
    }
}
