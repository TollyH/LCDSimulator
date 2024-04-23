namespace LCDSimulator
{
    public class DDRAM
    {
        private readonly byte[] data = new byte[80];

        public byte this[byte address]
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

        private void ProcessAddress(ref byte address)
        {
            if (address is >= 80 or (>= 40 and < 64))
            {
                throw new IndexOutOfRangeException("Index must be less than 80 and not be between 40 and 64.");
            }
            // Second line of screen starts at address 64, but lines are only 40 bytes long,
            // meaning DDRAM address 64 maps to internal array address 40
            if (address >= 64)
            {
                address -= 24;
            }
        }
    }
}
