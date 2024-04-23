namespace LCDSimulator
{
    public class DDRAM
    {
        private readonly byte[] data = new byte[DisplayController.TotalCharacterCount];

        public byte this[int address]
        {
            get
            {
                ProcessAddress(ref address);
                return data[address];
            }
            set
            {
                ProcessAddress(ref address);
                data[address] = value;
            }
        }

        private static void ProcessAddress(ref int address)
        {
            if (address is < 0 or >= DisplayController.TotalCharacterCount
                or (>= DisplayController.CharactersPerLine and < DisplayController.SecondLineStartAddress))
            {
                throw new IndexOutOfRangeException("Index must be less than 80 and not be between 40 and 64.");
            }
            // Second line of screen starts at address 64, but lines are only 40 bytes long,
            // meaning DDRAM address 64 maps to internal array address 40
            if (address >= DisplayController.SecondLineStartAddress)
            {
                address -= DisplayController.SecondLineStartAddress - DisplayController.CharactersPerLine;
            }
        }
    }
}
