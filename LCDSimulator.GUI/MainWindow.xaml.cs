using System.Windows;
using System.Windows.Controls;

namespace LCDSimulator.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool runUncheckHandler = true;

        public MainWindow()
        {
            InitializeComponent();
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

        private void SizeMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            // One size menu item must always be checked
            if (runUncheckHandler && sender is MenuItem item)
            {
                item.IsChecked = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sizeMenu.Items.OfType<MenuItem>().First().IsChecked = true;
        }
    }
}