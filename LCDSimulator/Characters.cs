using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LCDSimulator
{
    public static class Characters
    {
        public const int MaximumImageWidth = 8;
        public const int MaximumImageHeight = 16;

        public static readonly string CharacterRootFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "CharacterImages");

        public static readonly string[] CharacterImagePaths = new string[256]
        {
            // 0000 0000
            "Blank",
            // 0000 0001
            "Blank",
            // 0000 0010
            "Blank",
            // 0000 0011
            "Blank",
            // 0000 0100
            "Blank",
            // 0000 0101
            "Blank",
            // 0000 0110
            "Blank",
            // 0000 0111
            "Blank",
            // 0000 1000
            "Blank",
            // 0000 1001
            "Blank",
            // 0000 1010
            "Blank",
            // 0000 1011
            "Blank",
            // 0000 1100
            "Blank",
            // 0000 1101
            "Blank",
            // 0000 1110
            "Blank",
            // 0000 1111
            "Blank",
            // 0001 0000
            "Blank",
            // 0001 0001
            "Blank",
            // 0001 0010
            "Blank",
            // 0001 0011
            "Blank",
            // 0001 0100
            "Blank",
            // 0001 0101
            "Blank",
            // 0001 0110
            "Blank",
            // 0001 0111
            "Blank",
            // 0001 1000
            "Blank",
            // 0001 1001
            "Blank",
            // 0001 1010
            "Blank",
            // 0001 1011
            "Blank",
            // 0001 1100
            "Blank",
            // 0001 1101
            "Blank",
            // 0001 1110
            "Blank",
            // 0001 1111
            "Blank",
            // 0010 0000
            "Blank",
            // 0010 0001
            Path.Join("Low", "Exclamation"),
            // 0010 0010
            Path.Join("Low", "QuoteDouble"),
            // 0010 0011
            Path.Join("Low", "Hash"),
            // 0010 0100
            Path.Join("Low", "Dollar"),
            // 0010 0101
            Path.Join("Low", "Percent"),
            // 0010 0110
            Path.Join("Low", "Ampersand"),
            // 0010 0111
            Path.Join("Low", "QuoteSingle"),
            // 0010 1000
            Path.Join("Low", "BracketOpen"),
            // 0010 1001
            Path.Join("Low", "BracketClose"),
            // 0010 1010
            Path.Join("Low", "Asterisk"),
            // 0010 1011
            Path.Join("Low", "Plus"),
            // 0010 1100
            Path.Join("Low", "Comma"),
            // 0010 1101
            Path.Join("Low", "Dash"),
            // 0010 1110
            Path.Join("Low", "Dot"),
            // 0010 1111
            Path.Join("Low", "Slash"),
            // 0011 0000
            Path.Join("Low", "0"),
            // 0011 0001
            Path.Join("Low", "1"),
            // 0011 0010
            Path.Join("Low", "2"),
            // 0011 0011
            Path.Join("Low", "3"),
            // 0011 0100
            Path.Join("Low", "4"),
            // 0011 0101
            Path.Join("Low", "5"),
            // 0011 0110
            Path.Join("Low", "6"),
            // 0011 0111
            Path.Join("Low", "7"),
            // 0011 1000
            Path.Join("Low", "8"),
            // 0011 1001
            Path.Join("Low", "9"),
            // 0011 1010
            Path.Join("Low", "Colon"),
            // 0011 1011
            Path.Join("Low", "Semicolon"),
            // 0011 1100
            Path.Join("Low", "LessThan"),
            // 0011 1101
            Path.Join("Low", "Equals"),
            // 0011 1110
            Path.Join("Low", "GreaterThan"),
            // 0011 1111
            Path.Join("Low", "Question"),
            // 0100 0000
            Path.Join("Low", "At"),
            // 0100 0001
            Path.Join("Low", "A"),
            // 0100 0010
            Path.Join("Low", "B"),
            // 0100 0011
            Path.Join("Low", "C"),
            // 0100 0100
            Path.Join("Low", "D"),
            // 0100 0101
            Path.Join("Low", "E"),
            // 0100 0110
            Path.Join("Low", "F"),
            // 0100 0111
            Path.Join("Low", "G"),
            // 0100 1000
            Path.Join("Low", "H"),
            // 0100 1001
            Path.Join("Low", "I"),
            // 0100 1010
            Path.Join("Low", "J"),
            // 0100 1011
            Path.Join("Low", "K"),
            // 0100 1100
            Path.Join("Low", "L"),
            // 0100 1101
            Path.Join("Low", "M"),
            // 0100 1110
            Path.Join("Low", "N"),
            // 0100 1111
            Path.Join("Low", "O"),
            // 0101 0000
            Path.Join("Low", "P"),
            // 0101 0001
            Path.Join("Low", "Q"),
            // 0101 0010
            Path.Join("Low", "R"),
            // 0101 0011
            Path.Join("Low", "S"),
            // 0101 0100
            Path.Join("Low", "T"),
            // 0101 0101
            Path.Join("Low", "U"),
            // 0101 0110
            Path.Join("Low", "V"),
            // 0101 0111
            Path.Join("Low", "W"),
            // 0101 1000
            Path.Join("Low", "X"),
            // 0101 1001
            Path.Join("Low", "Y"),
            // 0101 1010
            Path.Join("Low", "Z"),
            // 0101 1011
            Path.Join("Low", "SquareBracketOpen"),
            // 0101 1100
            Path.Join("Low", "Yen"),
            // 0101 1101
            Path.Join("Low", "SquareBracketClose"),
            // 0101 1110
            Path.Join("Low", "Caret"),
            // 0101 1111
            Path.Join("Low", "Underscore"),
            // 0110 0000
            Path.Join("Low", "Backtick"),
            // 0110 0001
            Path.Join("Low", "A-"),
            // 0110 0010
            Path.Join("Low", "B-"),
            // 0110 0011
            Path.Join("Low", "C-"),
            // 0110 0100
            Path.Join("Low", "D-"),
            // 0110 0101
            Path.Join("Low", "E-"),
            // 0110 0110
            Path.Join("Low", "F-"),
            // 0110 0111
            Path.Join("Low", "G-"),
            // 0110 1000
            Path.Join("Low", "H-"),
            // 0110 1001
            Path.Join("Low", "I-"),
            // 0110 1010
            Path.Join("Low", "J-"),
            // 0110 1011
            Path.Join("Low", "K-"),
            // 0110 1100
            Path.Join("Low", "L-"),
            // 0110 1101
            Path.Join("Low", "M-"),
            // 0110 1110
            Path.Join("Low", "N-"),
            // 0110 1111
            Path.Join("Low", "O-"),
            // 0111 0000
            Path.Join("Low", "P-"),
            // 0111 0001
            Path.Join("Low", "Q-"),
            // 0111 0010
            Path.Join("Low", "R-"),
            // 0111 0011
            Path.Join("Low", "S-"),
            // 0111 0100
            Path.Join("Low", "T-"),
            // 0111 0101
            Path.Join("Low", "U-"),
            // 0111 0110
            Path.Join("Low", "V-"),
            // 0111 0111
            Path.Join("Low", "W-"),
            // 0111 1000
            Path.Join("Low", "X-"),
            // 0111 1001
            Path.Join("Low", "Y-"),
            // 0111 1010
            Path.Join("Low", "Z-"),
            // 0111 1011
            Path.Join("Low", "CurlyBracketOpen"),
            // 0111 1100
            Path.Join("Low", "Pipe"),
            // 0111 1101
            Path.Join("Low", "CurlyBracketClose"),
            // 0111 1110
            Path.Join("Low", "ArrowRight"),
            // 0111 1111
            Path.Join("Low", "ArrowLeft"),
            // 1000 0000
            "Blank",
            // 1000 0001
            "Blank",
            // 1000 0010
            "Blank",
            // 1000 0011
            "Blank",
            // 1000 0100
            "Blank",
            // 1000 0101
            "Blank",
            // 1000 0110
            "Blank",
            // 1000 0111
            "Blank",
            // 1000 1000
            "Blank",
            // 1000 1001
            "Blank",
            // 1000 1010
            "Blank",
            // 1000 1011
            "Blank",
            // 1000 1100
            "Blank",
            // 1000 1101
            "Blank",
            // 1000 1110
            "Blank",
            // 1000 1111
            "Blank",
            // 1001 0000
            "Blank",
            // 1001 0001
            "Blank",
            // 1001 0010
            "Blank",
            // 1001 0011
            "Blank",
            // 1001 0100
            "Blank",
            // 1001 0101
            "Blank",
            // 1001 0110
            "Blank",
            // 1001 0111
            "Blank",
            // 1001 1000
            "Blank",
            // 1001 1001
            "Blank",
            // 1001 1010
            "Blank",
            // 1001 1011
            "Blank",
            // 1001 1100
            "Blank",
            // 1001 1101
            "Blank",
            // 1001 1110
            "Blank",
            // 1001 1111
            "Blank",
            // 1010 0000
            "Blank",
            // 1010 0001
            Path.Join("High", "Dot"),
            // 1010 0010
            Path.Join("High", "QuoteOpen"),
            // 1010 0011
            Path.Join("High", "QuoteClose"),
            // 1010 0100
            Path.Join("High", "Comma"),
            // 1010 0101
            Path.Join("High", "Interpunct"),
            // 1010 0110
            Path.Join("High", "Wo"),
            // 1010 0111
            Path.Join("High", "ASmall"),
            // 1010 1000
            Path.Join("High", "ISmall"),
            // 1010 1001
            Path.Join("High", "USmall"),
            // 1010 1010
            Path.Join("High", "ESmall"),
            // 1010 1011
            Path.Join("High", "OSmall"),
            // 1010 1100
            Path.Join("High", "YaSmall"),
            // 1010 1101
            Path.Join("High", "YuSmall"),
            // 1010 1110
            Path.Join("High", "YoSmall"),
            // 1010 1111
            Path.Join("High", "TsuSmall"),
            // 1011 0000
            Path.Join("High", "Dash"),
            // 1011 0001
            Path.Join("High", "A"),
            // 1011 0010
            Path.Join("High", "I"),
            // 1011 0011
            Path.Join("High", "U"),
            // 1011 0100
            Path.Join("High", "E"),
            // 1011 0101
            Path.Join("High", "O"),
            // 1011 0110
            Path.Join("High", "Ka"),
            // 1011 0111
            Path.Join("High", "Ki"),
            // 1011 1000
            Path.Join("High", "Ku"),
            // 1011 1001
            Path.Join("High", "Ke"),
            // 1011 1010
            Path.Join("High", "Ko"),
            // 1011 1011
            Path.Join("High", "Sa"),
            // 1011 1100
            Path.Join("High", "Shi"),
            // 1011 1101
            Path.Join("High", "Su"),
            // 1011 1110
            Path.Join("High", "Se"),
            // 1011 1111
            Path.Join("High", "So"),
            // 1100 0000
            Path.Join("High", "Ta"),
            // 1100 0001
            Path.Join("High", "Chi"),
            // 1100 0010
            Path.Join("High", "Tsu"),
            // 1100 0011
            Path.Join("High", "Te"),
            // 1100 0100
            Path.Join("High", "To"),
            // 1100 0101
            Path.Join("High", "Na"),
            // 1100 0110
            Path.Join("High", "Ni"),
            // 1100 0111
            Path.Join("High", "Nu"),
            // 1100 1000
            Path.Join("High", "Ne"),
            // 1100 1001
            Path.Join("High", "No"),
            // 1100 1010
            Path.Join("High", "Ha"),
            // 1100 1011
            Path.Join("High", "Hi"),
            // 1100 1100
            Path.Join("High", "Fu"),
            // 1100 1101
            Path.Join("High", "He"),
            // 1100 1110
            Path.Join("High", "Ho"),
            // 1100 1111
            Path.Join("High", "Ma"),
            // 1101 0000
            Path.Join("High", "Mi"),
            // 1101 0001
            Path.Join("High", "Mu"),
            // 1101 0010
            Path.Join("High", "Me"),
            // 1101 0011
            Path.Join("High", "Mo"),
            // 1101 0100
            Path.Join("High", "Ya"),
            // 1101 0101
            Path.Join("High", "Yu"),
            // 1101 0110
            Path.Join("High", "Yo"),
            // 1101 0111
            Path.Join("High", "Ra"),
            // 1101 1000
            Path.Join("High", "Ri"),
            // 1101 1001
            Path.Join("High", "Ru"),
            // 1101 1010
            Path.Join("High", "Re"),
            // 1101 1011
            Path.Join("High", "Ro"),
            // 1101 1100
            Path.Join("High", "Wa"),
            // 1101 1101
            Path.Join("High", "N"),
            // 1101 1110
            Path.Join("High", "Dakuten"),
            // 1101 1111
            Path.Join("High", "Handakuten"),
            // 1110 0000
            Path.Join("High5x10", "Alpha"),
            // 1110 0001
            Path.Join("High5x10", "AUmlaut"),
            // 1110 0010
            Path.Join("High5x10", "Beta"),
            // 1110 0011
            Path.Join("High5x10", "Epsilon"),
            // 1110 0100
            Path.Join("High5x10", "Mu"),
            // 1110 0101
            Path.Join("High5x10", "Sigma"),
            // 1110 0110
            Path.Join("High5x10", "Rho"),
            // 1110 0111
            Path.Join("High5x10", "G"),
            // 1110 1000
            Path.Join("High5x10", "SquareRoot"),
            // 1110 1001
            Path.Join("High5x10", "SuperscriptNegative1"),
            // 1110 1010
            Path.Join("High5x10", "J"),
            // 1110 1011
            Path.Join("High5x10", "SuperscriptAsterisk"),
            // 1110 1100
            Path.Join("High5x10", "Cent"),
            // 1110 1101
            Path.Join("High5x10", "Pound"),
            // 1110 1110
            Path.Join("High5x10", "NTilde"),
            // 1110 1111
            Path.Join("High5x10", "OUmlaut"),
            // 1111 0000
            Path.Join("High5x10", "P"),
            // 1111 0001
            Path.Join("High5x10", "Q"),
            // 1111 0010
            Path.Join("High5x10", "Theta"),
            // 1111 0011
            Path.Join("High5x10", "Infinity"),
            // 1111 0100
            Path.Join("High5x10", "Omega"),
            // 1111 0101
            Path.Join("High5x10", "UUmlaut"),
            // 1111 0110
            Path.Join("High5x10", "SigmaLarge"),
            // 1111 0111
            Path.Join("High5x10", "Pi"),
            // 1111 1000
            Path.Join("High5x10", "XMean"),
            // 1111 1001
            Path.Join("High5x10", "Y"),
            // 1111 1010
            Path.Join("High5x10", "Thousand"),
            // 1111 1011
            Path.Join("High5x10", "TenThousand"),
            // 1111 1100
            Path.Join("High5x10", "Yen"),
            // 1111 1101
            Path.Join("High5x10", "Divide"),
            // 1111 1110
            "Blank",
            // 1111 1111
            Path.Join("High5x10", "Block")
        };

        public static void LoadPixelData(byte[] pixelDataTarget)
        {
            if (pixelDataTarget.Length < CharacterImagePaths.Length * 16)
            {
                throw new ArgumentException($"Target array length must be at least {CharacterImagePaths.Length * 16}.");
            }

            for (int i = 0; i < CharacterImagePaths.Length; i++)
            {
                GetImageData(CharacterImagePaths[i]).CopyTo(pixelDataTarget, i * pixelDataTarget.Length);
            }
        }

        private static byte[] GetImageData(string path)
        {
            using Image<Rgb24> image = Image.Load<Rgb24>(Path.Join(CharacterRootFolder, path));

            if (image.Width > MaximumImageWidth || image.Height > MaximumImageHeight)
            {
                throw new ArgumentException("Image must be at most 8x16 pixels.");
            }

            byte[] data = new byte[MaximumImageHeight];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Rgb24 pixel = image[x, y];
                    if (pixel.R > 0 || pixel.G > 0 || pixel.B > 0)
                    {
                        // Images index from top-left, characters index from top-right
                        data[y] |= (byte)(1 << (MaximumImageWidth - x));
                    }
                }
            }

            return data;
        }
    }
}
