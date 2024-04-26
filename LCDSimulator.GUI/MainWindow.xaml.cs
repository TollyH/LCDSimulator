using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LCDSimulator.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Brush ScreenForeground { get; private set; }
        public Brush ScreenBackground { get; private set; }

        public DisplayController Controller { get; }

        private bool runUncheckHandler = true;

        public MainWindow()
        {
            InitializeComponent();

            MenuItem firstColorItem = colorMenu.Items.OfType<MenuItem>().First();

            ScreenForeground = firstColorItem.Foreground;
            ScreenBackground = firstColorItem.Background;

            Controller = new DisplayController();
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
                    _ = line.Children.Add(new Controls.LCDChar());
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
    }
}