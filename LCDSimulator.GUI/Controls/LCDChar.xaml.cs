using System.Windows.Controls;
using System.Windows.Shapes;

namespace LCDSimulator.GUI.Controls
{
    /// <summary>
    /// Interaction logic for LCDChar.xaml
    /// </summary>
    public partial class LCDChar : UserControl
    {
        public enum PixelState
        {
            Unpowered,
            PoweredOff,
            PoweredOn
        }

        public PixelState[,] Dots { get; } = new PixelState[5, 8];

        public bool SecondLine { get; set; }
        public int IndexOnLine { get; set; }

        private double _contrast = 0;
        public double Contrast
        {
            get => _contrast;
            set => _contrast = Math.Clamp(value, 0, 1);
        }

        private readonly Rectangle[,] dotRenderers = new Rectangle[5, 8];

        public LCDChar(bool secondLine, int indexOnLine)
        {
            InitializeComponent();

            for (int y = 0; y < Dots.GetLength(1); y++)
            {
                StackPanel row = (StackPanel)dotPanel.Children[y];
                for (int x = 0; x < Dots.GetLength(0); x++)
                {
                    dotRenderers[x, y] = (Rectangle)row.Children[x];
                }
            }

            SecondLine = secondLine;
            IndexOnLine = indexOnLine;

            UpdateCharacter();
        }

        public double CalculateOpacity(PixelState state)
        {
            if (state == PixelState.Unpowered)
            {
                return 0;
            }
            return Math.Clamp((state == PixelState.PoweredOn ? 0.9 : 0.1) + ((Contrast - 0.5) * 2), 0, 1);
        }

        public void UpdateCharacter()
        {
            for (int y = 0; y < Dots.GetLength(1); y++)
            {
                for (int x = 0; x < Dots.GetLength(0); x++)
                {
                    dotRenderers[x, y].Opacity = CalculateOpacity(Dots[x, y]);
                }
            }
        }
    }
}
