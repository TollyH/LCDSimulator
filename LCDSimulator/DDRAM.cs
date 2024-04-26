namespace LCDSimulator
{
    public class DDRAM
    {
        private readonly byte[] data = new byte[DisplayController.MaximumCharacterCount];

        public byte this[int address, bool twoLine]
        {
            get
            {
                ProcessAddress(ref address, twoLine);
                return data[address];
            }
            set
            {
                ProcessAddress(ref address, twoLine);
                data[address] = value;
            }
        }

        public void Reset()
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = DisplayController.BlankCharacter;
            }
        }

        private static void ProcessAddress(ref int address, bool twoLine)
        {
            byte maxAddress = (byte)(twoLine ? DisplayController.MaximumDDRAMAddress : DisplayController.MaximumCharacterCount - 1);
            if (address < 0 || address > maxAddress)
            {
                throw new IndexOutOfRangeException($"Index must greater than 0 and less than {maxAddress}.");
            }
            if (twoLine)
            {
                if (address is >= DisplayController.CharactersPerLine and < DisplayController.SecondLineStartAddress)
                {
                    throw new IndexOutOfRangeException($"Index must not be between {DisplayController.CharactersPerLine} and " +
                        $"{DisplayController.MaximumDDRAMAddress} in two line mode.");
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
}
