using System.Text;

namespace LCDSimulator.CLI
{
    public struct LCDPosition(byte line, byte offset)
    {
        public byte Line = line;
        public byte Offset = offset;
    }

    public struct LCDSize(byte width, byte height)
    {
        public byte Width = width;
        public byte Height = height;
    }

    public class DisplayInterface(DisplayController controller)
    {
        public DisplayController Controller { get; set; } = controller;

        /// <summary>
        /// Determine whether or not the LCD is currently busy and unable to respond to instructions.
        /// </summary>
        public bool IsBusy()
        {
            return (ReceiveData(false, false) & 0b10000000) != 0;
        }

        /// <summary>
        /// Receive an 8-bit value from the LCD display, with the RS pin either enabled or disabled,
        /// cycling the enable pin.
        /// </summary>
        /// <param name="registerSelect">
        /// <see langword="true"/> to read CGRAM/DDRAM data.
        /// <see langword="false"/> gets current address and busy flag,
        /// which should instead be retrieved with the <see cref="IsBusy"/> and <see cref="GetAddress"/> methods.</param>
        /// <param name="waitForNotBusy">
        /// Should be <see langword="true"/> unless the purpose of the call is to check the busy flag.
        /// </param>
        /// <returns></returns>
        public byte ReceiveData(bool registerSelect, bool waitForNotBusy)
        {
            while (waitForNotBusy && IsBusy()) { }

            Controller.ReadWrite = true;
            Controller.RegisterSelect = registerSelect;

            Controller.Enable = true;
            byte data = Controller.DataBus;
            Controller.Enable = false;

            return data;
        }

        /// <summary>
        /// Get the position of the cursor.
        /// </summary>
        /// <returns>
        /// line: 0-based line number between 0 and <see cref="MaxScreenHeight"/> - 1
        /// offset: 0-based position index between 0 and <see cref="MaxScreenWidth"/> - 1
        /// </returns>
        public LCDPosition GetCursorPosition(LCDSize size)
        {
            byte address = GetAddress();

            byte modSecondLine = (byte)(address % DisplayController.SecondLineStartAddress);
            byte line;
            if (modSecondLine >= size.Width)
            {
                line = (byte)(address >= DisplayController.SecondLineStartAddress ? 3 : 2);
            }
            else
            {
                line = (byte)(address >= DisplayController.SecondLineStartAddress ? 1 : 0);
            }

            return new LCDPosition(line, (byte)(modSecondLine % size.Width));
        }

        /// <summary>
        /// Read the text currently on the screen as a string.
        /// </summary>
        /// <returns>
        /// Custom characters are represented by \x01 through \x08 inclusive.
        /// Lines are separated by \n.
        /// </returns>
        public string Read(LCDSize size)
        {
            // Store old DDRAM address to return to later
            byte oldAddress = GetAddress();

            StringBuilder builder = new();

            for (int y = 0; y < size.Height; y++)
            {
                SetCursorPosition(size, new LCDPosition((byte)y, 0));
                for (int x = 0; x < size.Width; x++)
                {
                    // Get character data
                    // (display will automatically move to next character)
                    byte data = ReceiveData(true, true);
                    if (data <= 7)
                    {
                        // Convert 0-indexed custom character to 1-indexed
                        data++;
                    }
                    _ = builder.Append((char)data);
                }
                _ = builder.Append('\n');
            }

            // Restore DDRAM address
            SetDDRAMAddress(oldAddress);

            return builder.ToString();
        }

        /// <summary>
        /// Retrieve pixels for a defined custom character.
        /// </summary>
        /// <param name="charNumber">Character number between 0 and 7.</param>
        /// <returns>
        /// The lowest bit of each value corresponds to the rightmost pixel
        /// of each row of the character, starting at the top.
        /// Values will be no greater than 0b11111.
        /// </returns>
        public byte[] GetCustomChar(byte charNumber)
        {
            // Store old DDRAM address to return to later
            byte oldAddress = GetAddress();

            byte[] pixels = new byte[8];

            // Set address in CGRAM to that of address for this character
            SetCGRAMAddress((byte)(charNumber * 8));
            for (int i = 0; i < 8; i++)
            {
                // Get character line data
                // (display will automatically move to next line in character)
                pixels[i] = ReceiveData(true, true);
            }

            // Restore DDRAM address
            SetDDRAMAddress(oldAddress);

            return pixels;
        }

        /// <summary>
        /// Transmit an 8-bit value to the LCD display, with the RS pin either enabled or disabled,
        /// cycling the enable pin.
        /// </summary>
        public void TransmitData(bool registerSelect, byte data)
        {
            while (IsBusy()) { }

            Controller.ReadWrite = false;
            Controller.RegisterSelect = registerSelect;

            Controller.DataBus = data;
            CycleEnableLine();
        }

        /// <summary>
        /// Remove all characters from the display and return cursor to home.
        /// </summary>
        public void Clear()
        {
            TransmitData(false, 0b1);
        }

        /// <summary>
        /// Initialise the connected display. Must be used before display can be utilised.
        /// </summary>
        /// <param name="lines">false = 1 line, true = 2 lines</param>
        /// <param name="font">false = 5x8, true = 5x11</param>
        public void InitialiseDisplay(bool lines, bool font)
        {
            byte instruction = 0b110000;
            if (lines)
            {
                instruction |= 0b1000;
            }
            if (font)
            {
                instruction |= 0b100;
            }

            TransmitData(false, instruction);
            Clear();
        }

        /// <summary>
        /// Set the visibility of different aspects of the display.
        /// </summary>
        public void DisplaySet(bool display, bool cursor, bool blink)
        {
            byte instruction = 0b1000;
            if (display)
            {
                instruction |= 0b100;
            }
            if (cursor)
            {
                instruction |= 0b10;
            }
            if (blink)
            {
                instruction |= 0b1;
            }

            TransmitData(false, instruction);
        }

        /// <summary>
        /// Move either the cursor or the entire screen.
        /// </summary>
        /// <param name="cursorScreen">false = cursor, true = screen</param>
        /// <param name="leftRight">false = left, true = right</param>
        public void Scroll(bool cursorScreen, bool leftRight)
        {
            byte instruction = 0b10000;
            if (cursorScreen)
            {
                instruction |= 0b1000;
            }
            if (leftRight)
            {
                instruction |= 0b100;
            }

            TransmitData(false, instruction);
        }

        /// <summary>
        /// Return the cursor to the start of the screen.
        /// </summary>
        public void Home()
        {
            TransmitData(false, 0b10);
        }

        /// <summary>
        /// Turn the display backlight on or off.
        /// </summary>
        public void Backlight(bool power)
        {
            Controller.Backlight = power;
        }

        /// <summary>
        /// Set the position of the cursor.
        /// </summary>
        /// <param name="position">
        /// line: 0-based line number between 0 and <see cref="MaxScreenHeight"/> - 1
        /// offset: 0-based position index between 0 and <see cref="MaxScreenWidth"/> - 1
        /// </param>
        public void SetCursorPosition(LCDSize size, LCDPosition position)
        {
            if (position.Line % 2 != 0)
            {
                position.Offset += DisplayController.SecondLineStartAddress;
            }
            if (position.Line >= 2)
            {
                position.Offset += size.Width;
            }
            SetDDRAMAddress(position.Offset);
        }

        /// <summary>
        /// Write a string to the display starting at the current cursor position.
        /// </summary>
        /// <param name="message">
        /// Use \x01 through \x08 inclusive to insert custom characters.
        /// Use \n to move to the next line.
        /// </param>
        /// <remarks>Automatic line wrapping is handled by this function.</remarks>
        public void Write(LCDSize size, string message)
        {
            byte lastLine = GetCursorPosition(size).Line;
            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];
                if (c is >= '\x01' and <= '\x08')
                {
                    // Character should be a custom character.
                    // Convert 1-based index to 0-based.
                    c--;
                }
                if (c != '\n')
                {
                    TransmitData(true, (byte)c);
                }
                LCDPosition position = GetCursorPosition(size);
                if (c == '\n' || position.Offset >= size.Width || position.Line != lastLine)
                {
                    // Move to first character of next line
                    SetCursorPosition(size, new LCDPosition((byte)(++lastLine % size.Height), 0));
                }
            }
        }

        /// <summary>
        /// Define a custom character.
        /// </summary>
        /// <param name="charNumber">Character number between 0 and 7.</param>
        /// <param name="pixels">
        /// Must contain 8 byte values no greater than 0b11111 each.
        /// The lowest bit of each value corresponds to the rightmost pixel
        /// of each row of the character, starting at the top.
        /// </param>
        public void DefineCustomChar(byte charNumber, byte[] pixels)
        {
            // Store old DDRAM address to return to later
            byte oldAddress = GetAddress();

            // Set address in CGRAM to that of address for this character
            SetCGRAMAddress((byte)(charNumber * 8));
            for (int i = 0; i < 8; i++)
            {
                // Set character line data
                // (display will automatically move to next line in character)
                TransmitData(true, (byte)(pixels[i] & 0b11111));
            }

            // Restore DDRAM address
            SetDDRAMAddress(oldAddress);
        }

        private void CycleEnableLine()
        {
            Controller.Enable = true;
            Controller.Enable = false;
        }

        private byte GetAddress()
        {
            return (byte)(ReceiveData(false, true) & 0b1111111);
        }

        private void SetDDRAMAddress(byte address)
        {
            TransmitData(false, (byte)(0b10000000 | address));
        }

        private void SetCGRAMAddress(byte address)
        {
            TransmitData(false, (byte)(0b1000000 | address));
        }
    }
}
