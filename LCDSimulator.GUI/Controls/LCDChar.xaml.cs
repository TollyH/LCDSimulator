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

        public byte DDRAMAddress { get; set; }
        public bool SecondLine { get; set; }
        public int IndexOnLine { get; set; }

        private double _contrast = 0;
        public double Contrast
        {
            get => _contrast;
            set => _contrast = Math.Clamp(value, -1, 1);
        }

        public LCDChar(byte ddramAddress, bool secondLine, int indexOnLine)
        {
            InitializeComponent();

            Dots = new bool[5, 8];

            DDRAMAddress = ddramAddress;
            SecondLine = secondLine;
            IndexOnLine = indexOnLine;

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
