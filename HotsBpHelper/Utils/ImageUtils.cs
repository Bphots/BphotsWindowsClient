using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace HotsBpHelper.Utils
{
    public class ImageUtils : IImageUtil
    {
        public Bitmap CaptureScreen()
        {
            var screenBmp = new Bitmap(App.AppSetting.Position.Width, App.AppSetting.Position.Height, PixelFormat.Format32bppRgb);
            using (var bmpGraphics = Graphics.FromImage(screenBmp))
            {
                bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
                return screenBmp;
            }
        }

        public Bitmap CaptureScreen(int x1, int y1, int x2, int y2)
        {
            var screenBmp = new Bitmap(x2 - x1, y2 - y1, PixelFormat.Format32bppRgb);
            using (var bmpGraphics = Graphics.FromImage(screenBmp))
            {
                bmpGraphics.CopyFromScreen(x1, y1, 0, 0, screenBmp.Size);
                return screenBmp;
            }
        }

        public Bitmap CaptureWindow(int hwnd)
        {
            throw new NotImplementedException();
        }

        public Bitmap CaptureArea(Bitmap bmp, Rectangle rect, Point[] clipPoints, bool textInWhite = false)
        {
            Bitmap bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppRgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.FillRectangle(textInWhite ? Brushes.Black : Brushes.White, 0, 0, rect.Width, rect.Height);
                using (GraphicsPath graphicsPath = new GraphicsPath())
                {
                    if (clipPoints.Length != 0)
                    {
                        graphicsPath.AddPolygon(clipPoints);
                        graphics.SetClip(graphicsPath);
                    }
                }
                graphics.DrawImage(bmp, 0, 0, rect, GraphicsUnit.Pixel);
            }
            return bitmap;
        }

        public Bitmap CaptureBanArea(Rectangle rect)
        {
            var screenBmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppRgb);
            using (var bmpGraphics = Graphics.FromImage(screenBmp))
            {
                bmpGraphics.CopyFromScreen(rect.X, rect.Y, 0, 0, new Size(rect.Width, rect.Height));
                return screenBmp;
            } 
        }

        public Bitmap CopBitmap(Bitmap bmp, Rectangle cropArea)
        {
            var bmpImage = new Bitmap(bmp);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        public Bitmap RotateImage(Bitmap bmp, float angle)
        {
            int maxside = (int)(Math.Sqrt(bmp.Width * bmp.Width + bmp.Height * bmp.Height));
            //create a new empty bmp to hold rotated image
            Bitmap returnBitmap = new Bitmap(maxside, maxside);
            //make a graphics object from the empty bmp
            Graphics g = Graphics.FromImage(returnBitmap);


            //move rotation point to center of image
            g.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
            //rotate
            g.RotateTransform(angle);
            //move image back
            g.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
            //draw passed in image onto graphics object
            g.DrawImage(bmp, 10, 10);

            return returnBitmap;
        }

        public bool IsSimilarColor(Color color1, Color color2)
        {
            int offset = 80;
            return (Math.Abs(color1.R - color2.R) <= offset && Math.Abs(color1.G - color2.G) <= offset && Math.Abs(color1.B - color2.B) <= offset);
        }
    }
}