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

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Brush ScreenForeground { get; private set; }
        public Brush ScreenBackground { get; private set; }

        public DisplayController Controller { get; }

        private bool runUncheckHandler = true;

        private readonly System.Timers.Timer displayUpdateTimer = new(TimeSpan.FromMilliseconds(RefreshRateMilliseconds));

        private static readonly Brush pinIndicatorFill = Brushes.Green;

        public MainWindow()
        {
            InitializeComponent();

            MenuItem firstColorItem = colorMenu.Items.OfType<MenuItem>().First();

            ScreenForeground = firstColorItem.Foreground;
            ScreenBackground = firstColorItem.Background;

            Controller = new DisplayController();

            displayUpdateTimer.Elapsed += displayUpdateTimer_Elapsed;
            displayUpdateTimer.Start();
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
                    byte ddramAddress = indexOnLine;
                    if (secondLine)
                    {
                        ddramAddress += DisplayController.SecondLineStartAddress;
                    }

                    _ = line.Children.Add(new Controls.LCDChar(ddramAddress, secondLine, indexOnLine));
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

            foreach (StackPanel line in screenPanel.Children.OfType<StackPanel>())
            {
                foreach (Controls.LCDChar lcdChar in line.Children.OfType<Controls.LCDChar>())
                {
                    lcdChar.Foreground = foreground;
                }
            }

            ScreenForeground = foreground;
            ScreenBackground = background;
        }

        public void RefreshDisplayCharacters()
        {
            Controller.UpdateRenderedDots();

            foreach (StackPanel line in screenPanel.Children.OfType<StackPanel>())
            {
                foreach (Controls.LCDChar lcdChar in line.Children.OfType<Controls.LCDChar>())
                {
                    bool[,] sourceArray = lcdChar.SecondLine ? Controller.SecondLineDots : Controller.FirstLineDots;
                    int startX = lcdChar.IndexOnLine * DisplayController.DotsPerCharacterWidth;

                    for (int y = 0; y < DisplayController.DotsPerCharacterHeight; y++)
                    {
                        for (int x = 0; x < DisplayController.DotsPerCharacterWidth; x++)
                        {
                            lcdChar.Dots[x, y] = sourceArray[startX + x, y];
                        }
                    }

                    lcdChar.UpdateCharacter();
                }
            }
        }

        public void RefreshPinIndicators()
        {
            rsPinIndicator.Fill = Controller.RegisterSelect ? pinIndicatorFill : null;
            rwPinIndicator.Fill = Controller.ReadWrite ? pinIndicatorFill : null;
            ePinIndicator.Fill = Controller.Enable ? pinIndicatorFill : null;

            d0PinIndicator.Fill = (Controller.DataBus & 0b1) != 0 ? pinIndicatorFill : null;
            d1PinIndicator.Fill = (Controller.DataBus & 0b10) != 0 ? pinIndicatorFill : null;
            d2PinIndicator.Fill = (Controller.DataBus & 0b100) != 0 ? pinIndicatorFill : null;
            d3PinIndicator.Fill = (Controller.DataBus & 0b1000) != 0 ? pinIndicatorFill : null;
            d4PinIndicator.Fill = (Controller.DataBus & 0b10000) != 0 ? pinIndicatorFill : null;
            d5PinIndicator.Fill = (Controller.DataBus & 0b100000) != 0 ? pinIndicatorFill : null;
            d6PinIndicator.Fill = (Controller.DataBus & 0b1000000) != 0 ? pinIndicatorFill : null;
            d7PinIndicator.Fill = (Controller.DataBus & 0b10000000) != 0 ? pinIndicatorFill : null;
        }

        public void RefreshAllSimulatorComponents()
        {
            RefreshDisplayCharacters();
            RefreshPinIndicators();
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
        }

        private void displayUpdateTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(RefreshAllSimulatorComponents);
        }
    }
}