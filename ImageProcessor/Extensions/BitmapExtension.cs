using System;
using System.Drawing;
using System.Threading;
using ImageProcessor.ImageProcessing;

namespace ImageProcessor.Extensions
{
    public static class BitmapExtension
    {
        public static Color GetPixel(this FastBitmap fbitmap, int y, int x)
        {
            unsafe
            {
                byte* row = (byte*)fbitmap.Scan0, bb = row;
                for (var yy = 0; yy < y; yy++, bb = row += fbitmap.Stride)
                {
                    for (var xx = 0; xx < x; xx++, bb += fbitmap.PixelSize)
                    {
                        // *(bb + 0) is B (Blue ) component of the pixel
                        // *(bb + 1) is G (Green) component of the pixel
                        // *(bb + 2) is R (Red  ) component of the pixel
                        // *(bb + 3) is A (Alpha) component of the pixel ( for 32bpp )
                    }
                }
                
                return Color.FromArgb(*(bb + 0), *(bb + 1), *(bb + 2));
            }
        }

        public static Bitmap ToGrayscale(this Bitmap original)
        {
            //create a blank bitmap the same size as original
            var bmp = new Bitmap(original);

            using (var fbitmap = new FastBitmap(bmp, 0, 0, bmp.Width, bmp.Height))
            {
                unsafe
                {
                    byte* row = (byte*) fbitmap.Scan0, bb = row; 
                    for (var yy = 0; yy < fbitmap.Height; yy++, bb = row += fbitmap.Stride)
                    {
                        for (var xx = 0; xx < fbitmap.Width; xx++, bb += fbitmap.PixelSize)
                        {
                            // *(bb + 0) is B (Blue ) component of the pixel
                            // *(bb + 1) is G (Green) component of the pixel
                            // *(bb + 2) is R (Red  ) component of the pixel
                            // *(bb + 3) is A (Alpha) component of the pixel ( for 32bpp )
                            var gray = (byte) ((1140**(bb + 0) + 5870**(bb + 1) + 2989**(bb + 2))/10000);
                            *(bb + 0) = *(bb + 1) = *(bb + 2) = gray;
                        }
                    }
                }
            }
            return bmp;
        }

        public static int[,] GetGrayScaleMatrix(this Bitmap original)
        {
            var matrix = new int[original.Height, original.Width];
            using (var fbitmap = new FastBitmap(original, 0, 0, original.Width, original.Height))
            {
                unsafe
                {
                    byte* row = (byte*) fbitmap.Scan0, bb = row; 
                    for (var yy = 0; yy < fbitmap.Height; yy++, bb = row += fbitmap.Stride)
                    {
                        for (var xx = 0; xx < fbitmap.Width; xx++, bb += fbitmap.PixelSize)
                        {
                            var gray = (byte) ((1140**(bb + 0) + 5870**(bb + 1) + 2989**(bb + 2))/10000);
                            if (gray == 254)
                                gray = 255;
                            matrix[yy, xx] = gray;
                        }
                    }
                }
            }

            return matrix;
        }

        public static Bitmap Zoom(this Bitmap originalBitmap, double scale)
        {
            var zoomFactor = scale/100;
            var newSize = new Size((int) (originalBitmap.Width*zoomFactor), (int) (originalBitmap.Height*zoomFactor));
            var bmp = new Bitmap(originalBitmap, newSize);
            return bmp;
        }

        private static double NormalizeAngle(double angle)
        {
            var division = angle/(Math.PI/2);
            var fraction = Math.Ceiling(division) - division;

            return fraction*Math.PI/2;
        }

        public static Bitmap CropAtRect(this Bitmap b, Rectangle r)
        {
            var nb = new Bitmap(r.Width, r.Height);
            using (var g = Graphics.FromImage(nb))
                g.DrawImage(b, -r.X, -r.Y);
            return nb;
        }

        public static Bitmap RotateImage(this Bitmap rotateMe, float angle)
        {
            angle = -angle;
            var normalizedRotationAngle = NormalizeAngle(angle);
            double widthD = rotateMe.Width, heightD = rotateMe.Height;

            var newWidthD = Math.Cos(normalizedRotationAngle)*widthD + Math.Sin(normalizedRotationAngle)*heightD;
            var newHeightD = Math.Cos(normalizedRotationAngle)*heightD + Math.Sin(normalizedRotationAngle)*widthD;

            var newWidth = (int) Math.Ceiling(newWidthD);
            var newHeight = (int) Math.Ceiling(newHeightD);
            if (newWidth < newHeight)
            {
                var temp = newWidth;
                newWidth = newHeight;
                newHeight = temp;
            }

            using (var bmp = new Bitmap(newWidth, newHeight))
            {
                using (var g = Graphics.FromImage(bmp))
                    g.DrawImageUnscaled(rotateMe, (newWidth - rotateMe.Width) / 2, (newHeight - rotateMe.Height) / 2,
                        bmp.Width, bmp.Height);

                //Now, actually rotate the image
                var rotatedImage = new Bitmap(bmp.Width, bmp.Height);

                using (var g = Graphics.FromImage(rotatedImage))
                {
                    g.FillRectangle(Brushes.Black, 0, 0, rotatedImage.Width, rotatedImage.Height);
                    g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
                    //set the rotation point as the center into the matrix
                    g.RotateTransform(angle); //rotate
                    g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2); //restore rotation point into the matrix
                    g.DrawImage(bmp, new Point(0, 0)); //draw the image on the new bitmap
                }
                return rotatedImage;
            }

        }

        public static Bitmap Binarilization(this Bitmap grayScaledBitmap, int threshold)
        {
            var bmp = new Bitmap(grayScaledBitmap); // new Bitmap(grayScaledBitmap.Width, grayScaledBitmap.Height);
            using (var fbitmap = new FastBitmap(bmp, 0, 0, bmp.Width, bmp.Height))
            {
                unsafe
                {
                    byte* row = (byte*) fbitmap.Scan0, bb = row;
                    for (var yy = 0; yy < fbitmap.Height; yy++, bb = row += fbitmap.Stride)
                    {
                        for (var xx = 0; xx < fbitmap.Width; xx++, bb += fbitmap.PixelSize)
                        {
                            var gray = (byte) ((1140**(bb + 0) + 5870**(bb + 1) + 2989**(bb + 2))/10000);
                            *(bb + 0) = *(bb + 1) = *(bb + 2) = (byte) (gray > threshold ? 0 : 255);
                        }
                    }
                }
            }
            return bmp;
        }

        public static Bitmap BinarilizationWithValidation(this Bitmap grayScaledBitmap, int threshold, bool isLeft, out bool binarilizationValid)
        {
            var bmp = new Bitmap(grayScaledBitmap); // new Bitmap(grayScaledBitmap.Width, grayScaledBitmap.Height);
            using (var fbitmap = new FastBitmap(bmp, 0, 0, bmp.Width, bmp.Height))
            {
                unsafe
                {
                    int oneThirdSample = (int)(fbitmap.Height * fbitmap.Width * 0.3);
                    int countBlack = 0;
                    int[,] indexer = new int[fbitmap.Height, fbitmap.Width];
                    
                    byte* row = (byte*)fbitmap.Scan0, bb = row;
                    for (var yy = 0; yy < fbitmap.Height; yy++, bb = row += fbitmap.Stride)
                    {
                        for (var xx = 0; xx < fbitmap.Width; xx++, bb += fbitmap.PixelSize)
                        {
                            var gray = (byte) ((1140**(bb + 0) + 5870**(bb + 1) + 2989**(bb + 2))/10000);

                            if (isLeft)
                            {
                                if (xx > fbitmap.Width * 0.7 && gray > threshold)
                                    countBlack++;
                            }
                            if (!isLeft)
                            {
                                if (xx < fbitmap.Width * 0.3 && gray > threshold)
                                    countBlack++;
                            }
                            indexer[yy, xx] = gray > threshold ? 1 : 0;
                            * (bb + 0) = *(bb + 1) = *(bb + 2) = (byte) (gray > threshold ? 0 : 255);
                        }

                    }
                    
                    binarilizationValid = (double)countBlack / oneThirdSample < 0.8;
                    if ((double)countBlack / oneThirdSample > 0.01)
                    {
                        int y = (int) (fbitmap.Height * 0.33);
                        for (int x = 0; x < fbitmap.Width*0.5; ++x)
                        {
                            int countMiddle = 0;
                            bool passX = false;
                            for (int yy = y; yy < fbitmap.Height*0.66; ++yy)
                            {
                                for (int xx = x; xx < x + fbitmap.Width*0.3; ++xx)
                                {
                                    countMiddle += indexer[yy, xx];
                                    if (countMiddle > 0)
                                    {
                                        passX = true;
                                        break;
                                    }
                                }
                                if (passX)
                                    break;
                            }
                            if (passX)
                                continue;

                            binarilizationValid = false;
                            break;
                        }
                    }
                }
            }
            return bmp;
        }
    }
}