using System.Drawing;

namespace ImageProcessor.HashProcessing
{
    public class ImgHash
    {
        private const int Height = 9;

        private const int Width = 9;

        private readonly Bitmap _sourceImg;

        public ImgHash(Bitmap bmpimg)
        {
            _sourceImg = bmpimg;
        }

        public string GetHash()
        {
            using (var image = ReduceSize())
            {
                var grayValues = ReduceColor(image);
                var reslut = ComputeBits(grayValues);
                return reslut;
            }
        }

        private Bitmap ReduceSize()
        {
            var smallerbmp = new Bitmap(_sourceImg, new Size(Width, Height));
            return smallerbmp;
        }

        private int[] ReduceColor(Bitmap image)
        {
            int width = image.Width, height = image.Height;

            var grayValues = new int[width*height];

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var color = image.GetPixel(x, y);
                    var grayValue = (int) (color.R*0.299 + color.G*0.587 + color.B*0.114);
                    grayValues[y*height + x] = grayValue;
                }
            return grayValues;
        }

        private string ComputeBits(int[] values)
        {
            var result = "";
            var result16 = "";
            for (var i = 0; i <= 8; i++)
            {
                for (var j = 1; j <= 8; j++)
                {
                    if (values[i*9 + j] < values[i*9 + j - 1])
                        result += '1';
                    else
                        result += '0';
                }
            }

            char[] dy = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};
            var x = 0;
            for (var i = 0; i < result.Length; i++)
            {
                x = 2*x + result[i] - '0';
                if (i%4 == 3)
                {
                    result16 += dy[x];
                    x = 0;
                }
            }
            return result16;
        }
    }
}