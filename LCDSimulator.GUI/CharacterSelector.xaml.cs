using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LCDSimulator.GUI
{
    /// <summary>
    /// Interaction logic for CharacterSelector.xaml
    /// </summary>
    public partial class CharacterSelector : Window
    {
        public byte SelectedCharacterCode { get; private set; }

        public CharacterSelector()
        {
            InitializeComponent();

            for (int x = 0; x <= byte.MaxValue >> 4; x++)
            {
                for (int y = 0; y <= byte.MaxValue >> 4; y++)
                {
                    byte characterCode = (byte)((x << 4) | y);
                    string characterImagePath = Characters.GetImagePath(Characters.CharacterImagePaths[characterCode]);

                    Button characterButton = new()
                    {
                        Padding = new Thickness(2),
                        SnapsToDevicePixels = true,
                        MinWidth = 42,
                        Height = 42,
                        Tag = characterCode
                    };

                    if (characterCode < DisplayController.CGRAMCharacterCodeEnd)
                    {
                        characterButton.Content = new TextBlock()
                        {
                            Text = "CG\nRAM",
                            TextAlignment = TextAlignment.Center
                        };

                        characterButton.Background = Brushes.LightGreen;
                    }
                    else
                    {
                        Image contentImage = new()
                        {
                            Source = new BitmapImage(new Uri(characterImagePath))
                        };
                        RenderOptions.SetBitmapScalingMode(contentImage, BitmapScalingMode.NearestNeighbor);
                        characterButton.Content = contentImage;

                        characterButton.Background = Brushes.Transparent;
                    }

                    Grid.SetColumn(characterButton, x + 1);
                    Grid.SetRow(characterButton, y + 1);

                    characterButton.Click += CharacterButton_Click;

                    _ = selectorGrid.Children.Add(characterButton);
                }
            }
        }

        private void CharacterButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedCharacterCode = (byte)((Button)sender).Tag;
            DialogResult = true;
            Close();
        }
    }
}
