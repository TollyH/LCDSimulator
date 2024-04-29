namespace LCDSimulator.GUI
{
    internal static class Strings
    {
        public static readonly Dictionary<(bool RS, bool RW, byte Data, byte Mask), string> InstructionDescriptions = new()
        {
            { (false, false, 0b00000001, 0b11111111), "Clear display" },

            { (false, false, 0b00000010, 0b11111110), "Return home" },

            { (false, false, 0b00000100, 0b11111111), "Decrement cursor on read/write, don't shift screen" },
            { (false, false, 0b00000101, 0b11111111), "Decrement cursor on read/write, shift screen on write" },
            { (false, false, 0b00000110, 0b11111111), "Increment cursor on read/write, don't shift screen" },
            { (false, false, 0b00000111, 0b11111111), "Increment cursor on read/write, shift screen on write" },

            { (false, false, 0b00001000, 0b11111111), "Disable display, cursor, and blink" },
            { (false, false, 0b00001001, 0b11111111), "Disable display and cursor, enable blink" },
            { (false, false, 0b00001010, 0b11111111), "Disable display and blink, enable cursor" },
            { (false, false, 0b00001011, 0b11111111), "Disable display, enable cursor and blink" },
            { (false, false, 0b00001100, 0b11111111), "Enable display, disable cursor and blink" },
            { (false, false, 0b00001101, 0b11111111), "Enable display and blink, disable cursor" },
            { (false, false, 0b00001110, 0b11111111), "Enable display and cursor, disable blink" },
            { (false, false, 0b00001111, 0b11111111), "Enable display, cursor, and blink" },

            { (false, false, 0b00010000, 0b11111100), "Move cursor left" },
            { (false, false, 0b00010100, 0b11111100), "Move cursor right" },
            { (false, false, 0b00011000, 0b11111100), "Shift display left" },
            { (false, false, 0b00011100, 0b11111100), "Shift display right" },

            { (false, false, 0b00100000, 0b11111100), "4-bit interface, 1-line, 5x8 font" },
            { (false, false, 0b00100100, 0b11111100), "4-bit interface, 1-line, 5x10 font" },
            { (false, false, 0b00101000, 0b11111100), "4-bit interface, 2-line, 5x8 font" },
            { (false, false, 0b00110000, 0b11111100), "8-bit interface, 1-line, 5x8 font" },
            { (false, false, 0b00110100, 0b11111100), "8-bit interface, 1-line, 5x10 font" },
            { (false, false, 0b00111000, 0b11111100), "8-bit interface, 2-line, 5x8 font" },

            { (false, false, 0b01000000, 0b11000000), "Set address in CGRAM (address in lowest 6 bits)" },
            { (false, false, 0b10000000, 0b10000000), "Set address in DDRAM (address in lowest 7 bits)" },

            { (true, false, 0b00000000, 0b00000000), "Write data" },

            { (false, true, 0b00000000, 0b00000000), "Read busy flag and address counter" },

            { (true, true, 0b00000000, 0b00000000), "Read data" }
        };
    }
}
