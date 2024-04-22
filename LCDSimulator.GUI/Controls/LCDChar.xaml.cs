using System.Windows.Controls;
using System.Windows.Shapes;

namespace LCDSimulator.GUI.Controls
{
    /// <summary>
    /// Interaction logic for LCDChar.xaml
    /// </summary>
    public partial class LCDChar : UserControl
    {
        public bool[,] Dots { get; }

        private double _contrast = 0.0f;
        public double Contrast
        {
            get => _contrast;
            set => _contrast = Math.Clamp(value, -1, 1);
        }

        public LCDChar()
        {
            InitializeComponent();

            Dots = new bool[5, 8];

            UpdateCharacter();
        }

        public double CalculateOpacity(bool active)
        {
            return Math.Clamp((active ? 0.9 : 0.1) + Contrast, 0, 1);
        }

        public void UpdateCharacter()
        {
            for (int y = 0; y < Dots.GetLength(1); y++)
            {
                StackPanel row = (StackPanel)dotPanel.Children[y];
                for (int x = 0; x < Dots.GetLength(0); x++)
                {
                    Rectangle dot = (Rectangle)row.Children[x];
                    dot.Opacity = CalculateOpacity(Dots[x, y]);
                }
            }
        }
    }
}
