namespace LCDSimulator.CLI
{
    public class CommandLine(DisplayInterface displayInterface)
    {
        private const string prompt = "\n> ";

        private const byte maxScreenWidth = 40;
        private const byte maxScreenHeight = 4;
        private const byte maxScreenChars = 80;

        private LCDSize size = new(16, 2);

        public void StartCLI()
        {
            Console.WriteLine("LCD Simulator CLI interface. Commands start with #, i.e. \"#help\"");

            while (true)
            {
                Console.Write(prompt);

                string input = Console.ReadLine() ?? "";

                if (input.Length == 0)
                {
                    Console.WriteLine("You must enter either a command or text to write to the screen.");
                    continue;
                }

                if (input[0] == '#')
                {
                    // Command
                    string[] components = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string command = components[0];
                    string[] args = components[1..];

                    switch (command)
                    {
                        case "#exit":
                            return;
                        case "#help":
                            CommandHelp(args);
                            break;
                        case "#set_size":
                            CommandSetSize(args);
                            break;
                        case "#init":
                            CommandInit(args);
                            break;
                        case "#set":
                            CommandSet(args);
                            break;
                        case "#clear":
                            CommandClear(args);
                            break;
                        case "#home":
                            CommandHome(args);
                            break;
                        case "#scroll":
                            CommandScroll(args);
                            break;
                        case "#backlight":
                            CommandBacklight(args);
                            break;
                        case "#def_custom":
                            CommandDefCustom(args);
                            break;
                        case "#write_custom":
                            CommandWriteCustom(args);
                            break;
                        case "#read_custom":
                            CommandReadCustom(args);
                            break;
                        case "#newline":
                            CommandNewline(args);
                            break;
                        case "#setpos":
                            CommandSetpos(args);
                            break;
                        case "#getpos":
                            CommandGetpos(args);
                            break;
                        case "#read":
                            CommandRead(args);
                            break;
                        case "#raw_tx":
                            CommandRawTx(args);
                            break;
                        case "#raw_rx":
                            CommandRawRx(args);
                            break;
                        default:
                            Console.WriteLine($"\"{command}\" is not a recognised command. Run #help to see all available commands.");
                            break;
                    }
                }
                else
                {
                    // Text
                    displayInterface.Write(size, input);
                }
            }
        }

        private void CommandHelp(string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("The #help command takes no arguments.");
                return;
            }

            Console.WriteLine("LCD Simulator CLI interface. Commands start with #, i.e. \"#help\"\n"
                + "Write any text not prefixed with # to write it to the display\n"
                + "\nList of commands:\n"
                + "    #exit - Quit the CLI\n"
                + $"    #set_size [1-{maxScreenHeight}] [1-{maxScreenWidth}] - Set the number of lines and columns the display has\n"
                + "    #init 1/2 8/11 - Initialise the screen in (1)/(2) line mode with 5x(8) or 5x(11) font\n"
                + "    #set 0/1 0/1 0/1 - Set whether the display, cursor, and blinking are on (1) or off (0)\n"
                + "    #clear - Clear the screen of all characters and return the cursor to the start position\n"
                + "    #home - Return the cursor to the start position\n"
                + "    #scroll c/s l/r - Scroll the (c)ursor/(s)creen (l)eft/(r)ight\n"
                + "    #backlight 0/1 - Set the screen backlight on (1) or off (0)\n"
                + "    #def_custom [0-7] <char> - Define a custom character at index 0-7\n"
                + "        <char> is 8, 5-bit binary numbers separated by spaces\n"
                + "    #write_custom [0-7] - Write the custom character at index 0-7\n"
                + "    #read_custom [0-7] - Get the pixel data of the custom character at index 0-7\n"
                + "    #newline - Move the cursor to the start of the next line\n"
                + $"    #setpos [1-{size.Height}] [0-{size.Width - 1}] - Set the position of the cursor to a given line, at a 0-based offset\n"
                + "    #getpos - Get the position of the cursor\n"
                + "    #read - Read the text currently on the screen\n"
                + "    #raw_tx 0/1 <data> - (ADVANCED) Transmit raw data to the LCD module, with RS pin on (1) or off (0)\n"
                + "        <data> is an 8-bit binary number, going from D7-D0\n"
                + "    #raw_rx 0/1 - (ADVANCED) Receive raw data from the LCD module, with RS pin on (1) or off (0)");
        }

        private void CommandSetSize(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("The #set_size command requires two arguments.");
                return;
            }

            if (!byte.TryParse(args[0], out byte height) || height is < 1 or > maxScreenHeight)
            {
                Console.WriteLine($"The first argument to the #set_size command must be between 1 and {maxScreenHeight}.");
                return;
            }

            if (!byte.TryParse(args[1], out byte width) || width is < 1 or > maxScreenWidth)
            {
                Console.WriteLine($"The second argument to the #set_size command must be between 1 and {maxScreenWidth}.");
                return;
            }

            if (width * height > maxScreenChars)
            {
                Console.WriteLine($"The size of the screen cannot be greater than {maxScreenChars} characters.");
                return;
            }

            size.Height = height;
            size.Width = width;
        }

        private void CommandInit(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("The #init command requires two arguments.");
                return;
            }

            bool lines;
            switch (args[0])
            {
                case "1":
                    lines = false;
                    break;
                case "2":
                    lines = true;
                    break;
                default:
                    Console.WriteLine("The first argument to the #init command must be 1 or 2.");
                    return;
            }

            bool font;
            switch (args[1])
            {
                case "8":
                    font = false;
                    break;
                case "11":
                    font = true;
                    break;
                default:
                    Console.WriteLine("The second argument to the #init command must be 8 or 11.");
                    return;
            }

            displayInterface.InitialiseDisplay(lines, font);
        }

        private void CommandSet(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("The #set command requires three arguments.");
                return;
            }

            bool display;
            switch (args[0])
            {
                case "0":
                    display = false;
                    break;
                case "1":
                    display = true;
                    break;
                default:
                    Console.WriteLine("The first argument to the #set command must be 0 or 1.");
                    return;
            }

            bool cursor;
            switch (args[1])
            {
                case "0":
                    cursor = false;
                    break;
                case "1":
                    cursor = true;
                    break;
                default:
                    Console.WriteLine("The second argument to the #set command must be 0 or 1.");
                    return;
            }

            bool blink;
            switch (args[2])
            {
                case "0":
                    blink = false;
                    break;
                case "1":
                    blink = true;
                    break;
                default:
                    Console.WriteLine("The third argument to the #set command must be 0 or 1.");
                    return;
            }

            displayInterface.DisplaySet(display, cursor, blink);
        }

        private void CommandClear(string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("The #clear command takes no arguments.");
                return;
            }

            displayInterface.Clear();
        }

        private void CommandHome(string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("The #home command takes no arguments.");
                return;
            }

            displayInterface.Home();
        }

        private void CommandScroll(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("The #scroll command requires two arguments.");
                return;
            }

            bool cursorScreen;
            switch (args[0])
            {
                case "c":
                    cursorScreen = false;
                    break;
                case "s":
                    cursorScreen = true;
                    break;
                default:
                    Console.WriteLine("The first argument to the #scroll command must be c or s.");
                    return;
            }

            bool leftRight;
            switch (args[1])
            {
                case "l":
                    leftRight = false;
                    break;
                case "r":
                    leftRight = true;
                    break;
                default:
                    Console.WriteLine("The second argument to the #scroll command must be l or r.");
                    return;
            }

            displayInterface.Scroll(cursorScreen, leftRight);
        }

        private void CommandBacklight(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("The #backlight command requires one argument.");
                return;
            }

            bool backlight;
            switch (args[0])
            {
                case "0":
                    backlight = false;
                    break;
                case "1":
                    backlight = true;
                    break;
                default:
                    Console.WriteLine("The first argument to the #backlight command must be 0 or 1.");
                    return;
            }

            displayInterface.Backlight(backlight);
        }

        private void CommandDefCustom(string[] args)
        {
            if (args.Length != 9)
            {
                Console.WriteLine("The #def_custom command requires nine arguments.");
                return;
            }

            if (args[0].Length != 1)
            {
                Console.WriteLine("The first argument to the #def_custom command must be a single digit.");
                return;
            }
            // Convert ASCII digit to integer
            byte characterIndex = (byte)(args[0][0] - '0');
            if (characterIndex > 7)
            {
                Console.WriteLine("The first argument to the #def_custom command must be between 0 and 7.");
                return;
            }

            byte[] pixels = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                string binary = args[i + 1];
                if (binary.Length != 5)
                {
                    Console.WriteLine("Each binary argument to the #def_custom command must be five digits long.");
                    return;
                }

                try
                {
                    pixels[i] = Convert.ToByte(binary, 2);
                }
                catch
                {
                    Console.WriteLine("Each binary digit to the #def_custom command must be either 0 or 1.");
                    return;
                }
            }

            displayInterface.DefineCustomChar(characterIndex, pixels);
        }

        private void CommandWriteCustom(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("The #write_custom command requires one argument.");
                return;
            }

            if (args[0].Length != 1)
            {
                Console.WriteLine("The first argument to the #write_custom command must be a single digit.");
                return;
            }
            // Convert ASCII digit to integer
            byte characterIndex = (byte)(args[0][0] - '0');
            if (characterIndex > 7)
            {
                Console.WriteLine("The first argument to the #write_custom command must be between 0 and 7.");
                return;
            }

            // displayInterface.Write uses 1-based indexing
            displayInterface.Write(size, $"{(char)characterIndex + 1}");
        }

        private void CommandReadCustom(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("The #read_custom command requires one argument.");
                return;
            }

            if (args[0].Length != 1)
            {
                Console.WriteLine("The first argument to the #read_custom command must be a single digit.");
                return;
            }
            // Convert ASCII digit to integer
            byte characterIndex = (byte)(args[0][0] - '0');
            if (characterIndex > 7)
            {
                Console.WriteLine("The first argument to the #read_custom command must be between 0 and 7.");
                return;
            }

            byte[] pixels = displayInterface.GetCustomChar(characterIndex);
            foreach (byte row in pixels)
            {
                Console.Write($"{row:b5} ");
            }
            Console.WriteLine();
        }

        private void CommandNewline(string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("The #newline command takes no arguments.");
                return;
            }

            displayInterface.Write(size, "\n");
        }

        private void CommandSetpos(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("The #setpos command requires two arguments.");
                return;
            }

            if (!byte.TryParse(args[0], out byte line) || line < 1 || line > size.Height)
            {
                Console.WriteLine($"The first argument to the #setpos command must be between 1 and {size.Height}.");
                return;
            }
            // Convert to 0-indexed
            line--;

            if (!byte.TryParse(args[1], out byte offset) || offset >= size.Width)
            {
                Console.WriteLine($"The second argument to the #setpos command must be between 1 and {size.Width - 1}.");
                return;
            }

            displayInterface.SetCursorPosition(size, new LCDPosition(line, offset));
        }

        private void CommandGetpos(string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("The #getpos command takes no arguments.");
                return;
            }

            LCDPosition position = displayInterface.GetCursorPosition(size);
            Console.WriteLine($"line: {position.Line + 1}, offset: {position.Offset}");
        }

        private void CommandRead(string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("The #read command takes no arguments.");
                return;
            }

            string readString = displayInterface.Read(size);
            for (int i = 0; i < readString.Length; i++)
            {
                char c = readString[i];
                if (c is >= '\x01' and <= '\x08')
                {
                    // Convert custom character to printable placeholder
                    c = '#';
                }
                Console.Write(c);
            }
            Console.WriteLine();
        }

        private void CommandRawTx(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("The #raw_tx command requires two arguments.");
                return;
            }

            bool rsPin;
            switch (args[0])
            {
                case "0":
                    rsPin = false;
                    break;
                case "1":
                    rsPin = true;
                    break;
                default:
                    Console.WriteLine("The first argument to the #raw_tx command must be 0 or 1.");
                    return;
            }

            byte data;
            if (args[1].Length != 8)
            {
                Console.WriteLine("The second argument to the #raw_tx command must be eight digits long.");
                return;
            }
            try
            {
                data = Convert.ToByte(args[1], 2);
            }
            catch
            {
                Console.WriteLine("Each binary digit to the #raw_tx command must be either 0 or 1.");
                return;
            }

            displayInterface.TransmitData(rsPin, data);
        }

        private void CommandRawRx(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("The #raw_rx command requires one argument.");
                return;
            }

            bool rsPin;
            switch (args[0])
            {
                case "0":
                    rsPin = false;
                    break;
                case "1":
                    rsPin = true;
                    break;
                default:
                    Console.WriteLine("The first argument to the #raw_rx command must be 0 or 1.");
                    return;
            }

            byte data = displayInterface.ReceiveData(rsPin, true);
            Console.WriteLine($"{data:b8} (0x{data:x2}) ({data})");
        }
    }
}
