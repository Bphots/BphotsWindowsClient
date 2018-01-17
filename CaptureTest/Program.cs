using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using ImageProcessor;
using ImageProcessor.Extensions;
using ImageProcessor.ImageProcessing;

namespace CaptureTest
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var dir = @"D:\qqytqqyt\Documents\HeroesBpProject\OcrHelper\CaptureTest\bin\Debug\test\";
            //var dir = @"C:\Users\gdlcf\Desktop\HOS\客户端\HotsBpHelperOcr PoC version\第四版本\Debug\Debug\temp\";
            //using (var bitmap = new Bitmap(@"D:\qqytqqyt\Documents\HeroesBpProject\OcrHelper\HotsBpHelper\bin\Debug\Images\Heroes\3440x1440\color.bmp"))
            //using (var bitmapGray = bitmap.ToGrayscale())
            //using (var binarySample = bitmapGray.Binarilization(60))
            //{
            //    binarySample.Save(
            //        @"D:\qqytqqyt\Documents\HeroesBpProject\OcrHelper\HotsBpHelper\bin\Debug\Images\Heroes\3440x1440\zz.bmp");
            //}

            //if (args.Any())
            //{
            //    var recognizer1 = new Recognizer(@"H:\Project\HotsBpHelper\HotsBpHelper\HotsBpHelper\bin\Debug\Images\Heroes\");
            //    for (int i = 0; i < 1; ++i)
            //    {
            //        recognizer1.Recognize(
            //        @"D:\qqytqqyt\Documents\HeroesBpProject\OcrHelper\HotsBpHelper\bin\Debug\Images\Heroes\test.bmp", (float)29.7, new StringBuilder(), "1");
            //    }
            //    //FilePath file = args[0];
            //    //float angle = float.Parse(args[1]);
            //    //var sb = new StringBuilder();
            //    //(new Recognizer(file.GetDirPath())).Recognize(file, angle, sb);
            //    //Console.WriteLine(sb.ToString());
            //    //Console.ReadKey();
            //}
            //Console.WriteLine("OCR:");
            //Console.WriteLine(DateTime.Now);
            //var recognizer = new Recognizer(dir);
            //for (int i = 0; i < 100; ++i)
            //{
            //    var sb = new StringBuilder();
            //    recognizer.Recognize(
            //    dir + "test.bmp", (float)29.7, sb, "1");
            //    Console.Write(sb.ToString() + " ");
            //}
            //Console.WriteLine();
            //Console.WriteLine(DateTime.Now);

            ////Console.WriteLine("OCR:");
            ////Console.WriteLine(DateTime.Now);
            ////for (int i = 0; i < 100; ++i)
            ////{
            ////    var sb = new StringBuilder();
            ////    recognizer.Recognize(
            ////    @"C:\Users\gdlcf\Desktop\HOS\客户端\HotsBpHelperOcr PoC version\第四版本\Debug\Debug\temp\test2.bmp", (float)29.7, sb);
            ////    Console.Write(i.ToString() + " ");
            ////}
            ////Console.WriteLine();
            ////Console.WriteLine(DateTime.Now);

            ////Console.WriteLine("Screenshot:");
            ////Console.WriteLine(DateTime.Now);
            ////for (int i = 0; i < 100; ++i)
            ////{
            ////    using (var bitmap = new Bitmap(2736, 1824, PixelFormat.Format32bppRgb))
            ////    using (Graphics graphics = Graphics.FromImage(bitmap))
            ////    {
            ////        graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            ////        Console.Write(i + " ");
            ////    }
            ////}
            ////Console.WriteLine();
            ////Console.WriteLine(DateTime.Now);

            //Console.WriteLine("CroppedScreenshot:");
            //Console.WriteLine(DateTime.Now);
            //for (int i = 0; i < 100; ++i)
            //{
            //    using (var bitmap = CaptureScreen())
            //    {
            //        var rect = new Rectangle(20, 20, 130, 110);
            //        Point[] clipPoints = new[]
            //        {new Point(1, 1), new Point(1, 28), new Point(117, 91), new Point(117, 69)};
            //        using (var bitmap2 = CaptureArea(bitmap, rect, clipPoints))
            //        {
            //            bitmap2.Save(@"D:\qqytqqyt\Documents\HeroesBpProject\OcrHelper\CaptureTest\bin\Debug\test\" + i + ".bmp");
            //        }
            //        Console.Write(i + " ");
            //    }
            //}
            //Console.WriteLine();
            //Console.WriteLine(DateTime.Now);

            //recognizer.Dispose();
            Console.ReadLine();
            //ImageProcessingHelper.LookForBpStats(
            //    @"C:\Users\qqytqqyt\Documents\Tencent Files\199123107\FileRecv\20180106_051726.bmp");
            //var recognizer = new Recognizer(@"H:\Project\HotsBpHelper\HotsBpHelper\HotsBpHelper\bin\Debug\Images\Heroes\");
            //for (int i = 0; i < 1; ++i)
            //{
            //    recognizer.Recognize(
            //    @"D:\qqytqqyt\Documents\HeroesBpProject\OcrHelper\HotsBpHelper\bin\Debug\Images\Heroes\test.bmp", (float)29.7, new StringBuilder());
            //}
            //(new Recognizer(@"D:\qqytqqyt\Documents\HeroesBpProject\OcrHelper\HotsBpHelper\bin\Debug\Images\Heroes\")).Recognize(
            //    @"G:\4.tiff", (float)29.7, new StringBuilder(), "1");
        }
        public static Bitmap CaptureScreen()
        {
            Bitmap bitmap = new Bitmap(2736, 1824, PixelFormat.Format32bppRgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }
            return bitmap;
        }

        public static Bitmap CaptureArea(Bitmap bmp, Rectangle rect, Point[] clipPoints)
        {
            Bitmap bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppRgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.FillRectangle(Brushes.Black, 0, 0, rect.Width, rect.Height);
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

    }
}