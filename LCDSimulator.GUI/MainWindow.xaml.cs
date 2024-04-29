using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LCDSimulator.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        public const double RefreshRateMilliseconds = 100;  // 10 FPS

        public const double ContrastScaleSingleLine = 2;
        public const double ContrastScaleExtendedHeight = 16.0 / 11.0;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Brush ScreenForeground { get; private set; }
        public Brush ScreenBackground { get; private set; }

        public DisplayController Controller { get; }

        private double _contrast = 0;
        public double Contrast
        {
            get => _contrast;
            set => _contrast = Math.Clamp(value, 0, 1);
        }

        private readonly CLI.CommandLine commandLineInterface;

        private bool runUncheckHandler = true;

        private readonly List<Controls.LCDChar> lcdCharacters = new();

        private readonly System.Timers.Timer displayUpdateTimer = new(TimeSpan.FromMilliseconds(RefreshRateMilliseconds));

        private static readonly SolidColorBrush pinIndicatorFillHigh = Brushes.Green;
        private static readonly SolidColorBrush pinIndicatorFillLow = Brushes.Transparent;

        public MainWindow()
        {
            InitializeComponent();

            ConsoleWindow.Create();
            ConsoleWindow.SetVisibility(ConsoleWindow.Visibility.Hidden);

            if (Console.IsOutputRedirected || Console.IsInputRedirected)
            {
                // If another process has control of stdout and stdin after creating a console,
                // the console will be non-functional, so hide the option to show it.
                consoleItem.Visibility = Visibility.Collapsed;
            }

            MenuItem firstColorItem = colorMenu.Items.OfType<MenuItem>().First();

            ScreenForeground = firstColorItem.Foreground;
            ScreenBackground = firstColorItem.Background;

            Controller = new DisplayController();

            commandLineInterface = new CLI.CommandLine(new CLI.DisplayInterface(Controller));

            commandLineInterface.CommandProcessed += commandLineInterface_CommandProcessed;
            commandLineInterface.TextWritten += commandLineInterface_TextWritten;

            displayUpdateTimer.Elapsed += displayUpdateTimer_Elapsed;
        }

        ~MainWindow()
        {
            Dispose();
        }

        public void Dispose()
        {
            displayUpdateTimer.Dispose();

            GC.SuppressFinalize(this);
        }

        public void UpdateScreenSize(int width, int height)
        {
            if (width * height is > 80 or < 0)
            {
                throw new ArgumentException("There cannot be more than 80 or less than 0 characters on the screen");
            }

            screenPanel.Children.Clear();
            lcdCharacters.Clear();

            for (int y = 0; y < height; y++)
            {
                StackPanel line = new()
                {
                    Orientation = Orientation.Horizontal
                };
                for (int x = 0; x < width; x++)
                {
                    bool secondLine = y % 2 != 0;
                    byte indexOnLine = (byte)(y / 2 * width + x);

                    Controls.LCDChar lcdChar = new(secondLine, indexOnLine);
                    _ = line.Children.Add(lcdChar);
                    lcdCharacters.Add(lcdChar);
                }
                _ = screenPanel.Children.Add(line);
            }

            UpdateScreenColor(ScreenForeground, ScreenBackground);

            ScreenWidth = width;
            ScreenHeight = height;
        }

        public void UpdateScreenColor(Brush foreground, Brush background)
        {
            screenBorder.Background = background;

            foreach (Controls.LCDChar lcdChar in lcdCharacters)
            {
                lcdChar.Foreground = foreground;
            }

            ScreenForeground = foreground;
            ScreenBackground = background;
        }

        public void RefreshDisplayCharacters()
        {
            disabledBacklightOverlay.Visibility = Controller.Backlight
                ? Visibility.Hidden
                : Visibility.Visible;

            Controller.UpdateRenderedDots();

            double scaledContrast = Contrast;
            if (Controller.ExtendedCharacterHeight)
            {
                scaledContrast *= ContrastScaleExtendedHeight;
            }
            else if (!Controller.TwoLineMode)
            {
                scaledContrast *= ContrastScaleSingleLine;
            }

            foreach (Controls.LCDChar lcdChar in lcdCharacters)
            {
                lcdChar.Contrast = scaledContrast;

                bool[,] sourceArray = lcdChar.SecondLine
                    ? Controller.SecondLineDots
                    : Controller.FirstLineDots;
                byte[] addressArray = lcdChar.SecondLine
                    ? Controller.SecondLineDDRAMAddresses
                    : Controller.FirstLineDDRAMAddresses;

                if (Controller.TwoLineMode || !lcdChar.SecondLine)
                {
                    byte ddramAddress = addressArray[lcdChar.IndexOnLine];
                    byte characterCode = Controller.DisplayDataRAM[ddramAddress, Controller.TwoLineMode];

                    lcdChar.ToolTip = $"DDRAM address: {ddramAddress}\nCharacter code: {characterCode} ({characterCode:b8})";
                }
                else
                {
                    lcdChar.ToolTip = null;
                }

                int startX = lcdChar.IndexOnLine * DisplayController.DotsPerCharacterWidth;

                for (int y = 0; y < DisplayController.DotsPerCharacterHeight; y++)
                {
                    for (int x = 0; x < DisplayController.DotsPerCharacterWidth; x++)
                    {
                        // Dot should be completely invisible if it is on the second line in 1-line mode,
                        // unless it is the bottom part of a character in extended height mode.
                        bool invisible = !Controller.IsPowered
                            || (!Controller.TwoLineMode && lcdChar.SecondLine
                                && (!Controller.ExtendedCharacterHeight
                                    || y >= DisplayController.DotsInExtendedCharacterHeight));

                        lcdChar.Dots[x, y] = invisible
                            ? Controls.LCDChar.PixelState.Unpowered
                            : sourceArray[startX + x, y]
                                ? Controls.LCDChar.PixelState.PoweredOn
                                : Controls.LCDChar.PixelState.PoweredOff;
                    }
                }

                lcdChar.UpdateCharacter();
            }
        }

        public void RefreshPinIndicators()
        {
            vddPinIndicator.Fill = Controller.IsPowered ? pinIndicatorFillHigh : pinIndicatorFillLow;

            Color startColor = pinIndicatorFillLow.Color;
            Color endColor = pinIndicatorFillHigh.Color;
            // Blend between high and low color based on contrast amount
            voPinIndicator.Fill = new SolidColorBrush(new Color()
            {
                R = (byte)(startColor.R + ((endColor.R - startColor.R) * Contrast)),
                G = (byte)(startColor.G + ((endColor.G - startColor.G) * Contrast)),
                B = (byte)(startColor.B + ((endColor.B - startColor.B) * Contrast)),
                A = (byte)(startColor.A + ((endColor.A - startColor.A) * Contrast))
            });

            rsPinIndicator.Fill = Controller.RegisterSelect ? pinIndicatorFillHigh : pinIndicatorFillLow;
            rwPinIndicator.Fill = Controller.ReadWrite ? pinIndicatorFillHigh : pinIndicatorFillLow;
            ePinIndicator.Fill = Controller.Enable ? pinIndicatorFillHigh : pinIndicatorFillLow;

            d0PinIndicator.Fill = (Controller.DataBus & 0b1) != 0 ? pinIndicatorFillHigh : pinIndicatorFillLow;
            d1PinIndicator.Fill = (Controller.DataBus & 0b10) != 0 ? pinIndicatorFillHigh : pinIndicatorFillLow;
            d2PinIndicator.Fill = (Controller.DataBus & 0b100) != 0 ? pinIndicatorFillHigh : pinIndicatorFillLow;
            d3PinIndicator.Fill = (Controller.DataBus & 0b1000) != 0 ? pinIndicatorFillHigh : pinIndicatorFillLow;
            d4PinIndicator.Fill = (Controller.DataBus & 0b10000) != 0 ? pinIndicatorFillHigh : pinIndicatorFillLow;
            d5PinIndicator.Fill = (Controller.DataBus & 0b100000) != 0 ? pinIndicatorFillHigh : pinIndicatorFillLow;
            d6PinIndicator.Fill = (Controller.DataBus & 0b1000000) != 0 ? pinIndicatorFillHigh : pinIndicatorFillLow;
            d7PinIndicator.Fill = (Controller.DataBus & 0b10000000) != 0 ? pinIndicatorFillHigh : pinIndicatorFillLow;

            if (Controller.FourBitMode)
            {
                d0PinIndicator.Opacity = 0.25;
                d1PinIndicator.Opacity = 0.25;
                d2PinIndicator.Opacity = 0.25;
                d3PinIndicator.Opacity = 0.25;
            }
            else
            {
                d0PinIndicator.Opacity = 1;
                d1PinIndicator.Opacity = 1;
                d2PinIndicator.Opacity = 1;
                d3PinIndicator.Opacity = 1;
            }

            aPinIndicator.Fill = Controller.Backlight ? pinIndicatorFillHigh : pinIndicatorFillLow;
        }

        public void RefreshInternalInfoDisplays()
        {
            string currentRAM = Controller.AddressingCharacterGeneratorRAM ? "CGRAM" : "DDRAM";

            byte address = (byte)(Controller.AddressCounter & (Controller.AddressingCharacterGeneratorRAM
                ? DisplayController.CGRAMAddressMask
                : DisplayController.DDRAMAddressMask));
            addressCounterLabel.Content = $"{currentRAM} | {address} ({address:b8})";

            instructionList.IsEnabled = !Controller.FourBitMode;
        }

        public void RefreshAllSimulatorComponents()
        {
            RefreshDisplayCharacters();
            RefreshPinIndicators();
            RefreshInternalInfoDisplays();
        }

        private void SizeMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            runUncheckHandler = false;
            foreach (MenuItem item in sizeMenu.Items.OfType<MenuItem>())
            {
                item.IsChecked = ReferenceEquals(item, sender);
            }
            runUncheckHandler = true;
            if (sender is MenuItem selectedItem)
            {
                string[] split = ((string)selectedItem.Header).Split('x');
                UpdateScreenSize(int.Parse(split[0]), int.Parse(split[1]));
            }
        }

        private void ColorMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            runUncheckHandler = false;
            foreach (MenuItem item in colorMenu.Items.OfType<MenuItem>())
            {
                item.IsChecked = ReferenceEquals(item, sender);
            }
            runUncheckHandler = true;
            if (sender is MenuItem selectedItem)
            {
                UpdateScreenColor(selectedItem.Foreground, selectedItem.Background);
            }
        }

        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            // One item of this menu must always be checked
            if (runUncheckHandler && sender is MenuItem item)
            {
                item.IsChecked = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            colorMenu.Items.OfType<MenuItem>().First().IsChecked = true;
            sizeMenu.Items.OfType<MenuItem>().First().IsChecked = true;

            _ = Task.Run(commandLineInterface.StartCLI);
        }

        private void displayUpdateTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(RefreshAllSimulatorComponents);
        }

        private void vddPinIndicator_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Controller.IsPowered = !Controller.IsPowered;
            RefreshAllSimulatorComponents();
        }

        private void voPinIndicator_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Contrast += e.Delta / 5000.0;
            RefreshAllSimulatorComponents();
        }

        private void voPinIndicator_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _ = MessageBox.Show(this, "Use the scroll wheel on the VO pin to adjust contrast",
                "Contrast adjustment", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void rsPinIndicator_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Controller.RegisterSelect = !Controller.RegisterSelect;
            RefreshAllSimulatorComponents();
        }

        private void rwPinIndicator_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Controller.ReadWrite = !Controller.ReadWrite;
            RefreshAllSimulatorComponents();
        }

        private void ePinIndicator_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Controller.Enable = !Controller.Enable;
            RefreshAllSimulatorComponents();
        }

        private void aPinIndicator_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Controller.Backlight = !Controller.Backlight;
            RefreshAllSimulatorComponents();
        }

        private void DataPinIndicator_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                Controller.DataBus ^= (byte)(0b1 << int.Parse((string)element.Tag));
                RefreshAllSimulatorComponents();
            }
        }

        private void ContinuousRefreshItem_Checked(object sender, RoutedEventArgs e)
        {
            displayUpdateTimer.Start();
        }

        private void ContinuousRefreshItem_Unchecked(object sender, RoutedEventArgs e)
        {
            displayUpdateTimer.Stop();
        }

        private void screenBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            instructionList.MaxHeight = screenBorder.ActualHeight;
        }

        private void instructionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (instructionList.SelectedIndex == -1)
            {
                return;
            }

            string[] pinComponents = ((string)((ListBoxItem)instructionList.SelectedItem).Tag).Split(' ');

            Controller.RegisterSelect = pinComponents[0] != "0";
            Controller.ReadWrite = pinComponents[1] != "0";

            if (ReferenceEquals(instructionList.SelectedItem, writeDataItem) && !Controller.AddressingCharacterGeneratorRAM)
            {
                CharacterSelector selector = new()
                {
                    Owner = this
                };
                if (selector.ShowDialog() ?? false)
                {
                    Controller.DataBus = selector.SelectedCharacterCode;
                }
            }
            else
            {
                Controller.DataBus = Convert.ToByte(pinComponents[2], 2);
            }

            instructionList.SelectedIndex = -1;
        }

        private void consoleItem_Checked(object sender, RoutedEventArgs e)
        {
            ConsoleWindow.SetVisibility(ConsoleWindow.Visibility.Visible);
        }

        private void consoleItem_Unchecked(object sender, RoutedEventArgs e)
        {
            ConsoleWindow.SetVisibility(ConsoleWindow.Visibility.Hidden);
        }

        private void commandLineInterface_CommandProcessed(CLI.CommandLine sender, string command, string[] args)
        {
            Dispatcher.Invoke(RefreshAllSimulatorComponents);
        }

        private void commandLineInterface_TextWritten(CLI.CommandLine sender, string text)
        {
            Dispatcher.Invoke(RefreshAllSimulatorComponents);
        }
    }
}