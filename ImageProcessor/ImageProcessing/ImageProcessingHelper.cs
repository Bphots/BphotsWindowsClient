using System.Drawing;
using System.IO;
using DotNetHelper;
using ImageProcessor.Extensions;
using ImageProcessor.Ocr;

//using OpenCvSharp;
//using Size = OpenCvSharp.Size;

namespace ImageProcessor.ImageProcessing
{
    public static class ImageProcessingHelper
    {
        public static object GDILock = new object();


        public static bool CheckIfInBp(string file)
        {
            using (var bitmap = new Bitmap(file))
            return CheckIfInBp(bitmap);
        }

        public static bool CheckIfInBp(Bitmap bitmap)
        {
            using (var bitmapGray = bitmap.ToGrayscale())
            using (var binarySample = bitmapGray.Binarilization(75))
            {
                var matrix = binarySample.GetGrayScaleMatrix();
                var sampleLeft = (int) (0.007*binarySample.Height);
                var sum = 0;
                for (var y =(int) (binarySample.Height * 0.08); y < binarySample.Height*0.130; y++)
                {
                    int grayScale = matrix[y, sampleLeft];
                    if (grayScale != 255)
                        ++sum;
                }
                if (sum > 0)
                    return false;

                sum = 0;
                for (int y = (int) (binarySample.Height*0.130); y < binarySample.Height*0.168; y++)
                {
                    int grayScale = matrix[y, sampleLeft];
                    if (grayScale != 255)
                        ++sum;
                }
                if (sum < 0.2*binarySample.Height*0.038)
                    return false;

                sum = 0;
                for (int y = (int) (binarySample.Height*0.168); y < binarySample.Height*0.441; y++)
                {
                    int grayScale = matrix[y, sampleLeft];
                    if (grayScale != 255)
                        ++sum;
                }
                if (sum > 0)
                    return false;

                sum = 0;
                for (int y = (int) (binarySample.Height*0.441); y < binarySample.Height*0.482; y++)
                {
                    int grayScale = matrix[y, sampleLeft];
                    if (grayScale != 255)
                        ++sum;
                }
                if (sum < 0.2*binarySample.Height*0.038)
                    return false;

                sum = 0;
                for (int y = (int) (binarySample.Height*0.482); y < binarySample.Height*0.757; y++)
                {
                    int grayScale = matrix[y, sampleLeft];
                    if (grayScale != 255)
                        ++sum;
                }
                if (sum > 0)
                    return false;

                sum = 0;
                for (int y = (int) (binarySample.Height*0.757); y < binarySample.Height*0.795; y++)
                {
                    int grayScale = matrix[y, sampleLeft];
                    if (grayScale != 255)
                        ++sum;
                }
                if (sum < 0.2*binarySample.Height*0.038)
                    return false;

                sum = 0;
                for (int y = (int) (binarySample.Height*0.795); y < binarySample.Height; y++)
                {
                    int grayScale = matrix[y, sampleLeft];
                    if (grayScale != 255)
                        ++sum;
                }

                return sum == 0;
            }
        }

        public static int[] LookForPoints(string file)
        {
            using (var bitmap = new Bitmap(file))
            using (var bitmapGray = bitmap.ToGrayscale())
            using (var binarySample = bitmapGray.Binarilization(75))
            {
                var matrix = binarySample.GetGrayScaleMatrix();
                var leftX = 0;
                var sample = 0;
                for (var x = 10; x < binarySample.Width; x++)
                {
                    var sum = 0;
                    for (var y = 0; y < binarySample.Height / 4; y++)
                    {
                        var color = matrix[y, x];
                        if (color != 255)
                            ++sum;
                    }
                    if (sample == 0)
                        sample = sum;
                    else if (sample > sum * 3)
                    {
                        leftX = x + 1;
                        break;
                    }
                }

                var leftY = 0;
                for (int y = binarySample.Height / 4; y > 1; y--)
                {
                    var color = matrix[y, leftX - 2];
                    var colorConfirm = matrix[y - 1, leftX - 2];
                    if (color != 255 && colorConfirm != 255)
                    {
                        leftY = y + 1;
                        break;
                    }
                }

                var rightX = 0;
                sample = 0;
                for (int x = binarySample.Width - 10; x > 0; x--)
                {
                    var sum = 0;
                    for (var y = 0; y < binarySample.Height / 4; y++)
                    {
                        var color = matrix[y, x];
                        if (color != 255)
                            ++sum;
                    }
                    if (sample == 0)
                        sample = sum;
                    else if (sample > sum * 3)
                    {
                        rightX = x - 1;
                        break;
                    }
                }

                var rightY = 0;
                for (int y = binarySample.Height / 4; y > 1; y--)
                {
                    var color = matrix[y, rightX + 2];
                    var colorConfirm = matrix[y - 1, rightX + 2];
                    if (color != 255 && colorConfirm != 255)
                    {
                        rightY = y + 1;
                        break;
                    }
                }

                return new[] { leftX, leftY, rightX, rightY };
            }
        }

        public static int CheckMode(string file, float rotationAngle)
        {
            using (var bitmap = new Bitmap(file))
            using (var bitmapGray = bitmap.ToGrayscale())
            {
                using (var rotatedImage = bitmapGray.RotateImage(rotationAngle))
                {
                    if (OcrEngine.Debug)
                        rotatedImage.Save(Recognizer.TempDirectoryPath + "RotatedImage.bmp");
                    using (var thresholdedSample = rotatedImage.ReverseBinarilization(135))
                    {
                        if (OcrEngine.Debug)
                            thresholdedSample.Save(Recognizer.TempDirectoryPath + "Binary140CheckDarkMode.bmp");
                        int sampleWidth = thresholdedSample.Width / 5;
                        double maxPoints = sampleWidth * thresholdedSample.Height;
                        double sumBlackPoint = GetBlackPointSample(rotationAngle, thresholdedSample, sampleWidth);
                        if (sumBlackPoint / maxPoints < 0.01)
                        {
                            using (var newThresholdedSample = rotatedImage.ReverseBinarilization(80))
                            {
                                if (OcrEngine.Debug)
                                    newThresholdedSample.Save(Recognizer.TempDirectoryPath + "Binary80CheckDarkMode.bmp");
                                double blackPoint = GetBlackPointSample(rotationAngle, newThresholdedSample, sampleWidth);
                                if (blackPoint / maxPoints < 0.005)
                                    return -1;
                            }

                            return 0; // dark mode
                        }
                        if (sumBlackPoint / maxPoints > 0.3)
                            return -1; // suspicious...

                        return 1; // light mode
                    } 
                }
            }
           
        }

        public static int ProcessOnce(int threshold, Bitmap rotatedImage, FilePath tempFilePath, float rotation, bool textInWhite, out double count)
        {
            int firstThreshold = threshold;

            //firstThreshold = 125;
            firstThreshold = firstThreshold > 135 ? 135 : firstThreshold;
            firstThreshold = firstThreshold < 115 ? 115 : firstThreshold;

            using (var thresholdedSample = rotatedImage.ReverseBinarilization(firstThreshold, textInWhite))
            using (var croppedImage = CropImage(thresholdedSample, rotatedImage))
            {
                count = (double)croppedImage.Width / rotatedImage.Width * 7.5 + 0.15;
                bool binarilizationValid;
                using (var thresholdedCroppedImage = croppedImage.BinarilizationWithValidation(threshold, rotation > 0, out binarilizationValid, textInWhite))
                {
                    thresholdedCroppedImage.Save(tempFilePath);
                    if (!binarilizationValid)
                        return 0;

                    var segCount = GetSegmentation(thresholdedCroppedImage);

                    if (OcrEngine.Debug)
                        thresholdedCroppedImage.Save(Recognizer.TempDirectoryPath + "Threshold" + threshold + ".bmp");
                    return segCount;
                }
            }
        }

        public static Bitmap GetCroppedMap(FilePath file)
        {
            using (var bitmap = new Bitmap(file))
            using (var bitmapGray = bitmap.ToGrayscale())
            using (var thresholdedSample = bitmapGray.Binarilization(125))
            {
                Bitmap croppedImage = CropImage(thresholdedSample, thresholdedSample);
                return croppedImage;
            }
        }

        public static Bitmap GetCroppeddHero(FilePath file)
        {
            using (var bitmap = new Bitmap(file))
            using (var bitmapGray = bitmap.ToGrayscale())
            using (var thresholdedSample = bitmapGray.Binarilization(225))
            {
                Bitmap croppedImage = CropImage(thresholdedSample, thresholdedSample);
                return croppedImage;
            }
        }

        public static Bitmap GetCroppedImage(float rotationAngle, FilePath file, bool isDarkMode, out int sampleWidth)
        {
            using (var bitmap = new Bitmap(file))
            using (var bitmapGray = bitmap.ToGrayscale())
            using (var zoomedBitmapGray = bitmapGray.Zoom(200))
            using (var rotatedImage = zoomedBitmapGray.RotateImage(rotationAngle))
            using (var thresholdedSample = rotatedImage.Binarilization(isDarkMode ? 80 : 125))
            {
                sampleWidth = rotatedImage.Width;
                Bitmap croppedImage = CropImage(thresholdedSample, rotatedImage);
                return croppedImage;
            }
        }

        public static Bitmap GetRotatedImage(float rotationAngle, FilePath file, bool textInWhite, out int sampleWidth)
        {
            using (var bitmap = new Bitmap(file))
            using (var bitmapGray = bitmap.ToGrayscale())
            using (var zoomedBitmapGray = bitmapGray.Zoom(200))
            {
                var rotatedImage = zoomedBitmapGray.RotateImage(rotationAngle, textInWhite);
                sampleWidth = rotatedImage.Width;
                return rotatedImage;
            }
        }


        private static int[] GetHisto(Bitmap sample, int[,] indexer, int minRow, int maxRow)
        {
            int[] histo = new int[sample.Width];
            int height = maxRow - minRow;
            for (int x = 0; x < sample.Width; ++x)
            {
                histo[x] = 0;
                for (int y = minRow + (int)(height * 0.1); y < minRow + height * 0.9; ++y)
                {
                    var color = indexer[y, x];
                    if (color == 0)
                        ++histo[x];
                }
            }
            return histo;
        }

        private static Bitmap CropImage(Bitmap thresholdedSample, Bitmap rotatedImage)
        {
            var indexer = thresholdedSample.GetGrayScaleMatrix();
            var minRow = 0;
            for (int y = thresholdedSample.Height / 2; y > 0; --y)
            {
                var sum = 0;
                for (var x = 0; x < thresholdedSample.Width; x++)
                {
                    var color = indexer[y, x];
                    if (color == 0)
                        ++sum;
                }
                if (sum == 0)
                {
                    minRow = y;
                    break;
                }
            }

            int maxRow = thresholdedSample.Height - 1;
            for (int y = thresholdedSample.Height / 2; y < thresholdedSample.Height - 1; ++y)
            {
                var sum = 0;
                for (var x = 0; x < thresholdedSample.Width; x++)
                {
                    var color = indexer[y, x];
                    if (color == 0)
                        ++sum;
                }
                if (sum == 0)
                {
                    maxRow = y;
                    break;
                }
            }

            int[] histo = GetHisto(thresholdedSample, indexer, minRow, maxRow);
            
            var minColumn = 0;
            for (int i = 0; i < histo.Length; ++i)
            {
                if (histo[i] > 1)
                {
                    minColumn = i;
                    break;
                }
            }

            int maxColumn = minColumn;
            for (int x = histo.Length - 1; x > minColumn; --x)
            {
                if (histo[x] > 1)
                {
                    maxColumn = x;
                    break;
                }
            }

            return
                rotatedImage.CropAtRect(new Rectangle(minColumn > 2 ? minColumn - 2 : 0, minRow > 2 ? minRow - 2 : 0,
                    maxColumn - minColumn + 1 + 4, maxRow - minRow + 1 + 4));
        }

        private static int GetSegmentation(Bitmap mat4)
        {
            var count = 0;
            var indexer4 = mat4.GetGrayScaleMatrix();
            var lastGap = 0;
            var wasGap = true;
            for (var x = 0; x < mat4.Width; ++x)
            {
                var isChecked = false;
                for (var y = 0; y < mat4.Height; y++)
                {
                    var color = indexer4[y, x];
                    if (color != 255)
                    {
                        isChecked = true;
                        if (wasGap)
                        {
                            wasGap = false;
                            count++;
                            lastGap = x;
                            break;
                        }
                    }
                }
                if (isChecked || x - lastGap < 12)
                    continue;

                wasGap = true;
            }
            return count;
        }

        private static double GetBlackPointSample(float rotationAngle, Bitmap mat3, int sampleWidth)
        {
            double sumBlackPoint = 0;
            var indexerTemp = mat3.GetGrayScaleMatrix();
            if (rotationAngle > 0)
            {
                for (var y = 0; y < mat3.Height; ++y)
                {
                    for (var x = 0; x < sampleWidth; x++)
                    {
                        var color = indexerTemp[y, x];
                        if (color == 0)
                            sumBlackPoint++;
                    }
                }
            }
            else
            {
                for (var y = 0; y < mat3.Height; ++y)
                {
                    for (int x = mat3.Width - sampleWidth; x < mat3.Width; x++)
                    {
                        var color = indexerTemp[y, x];
                        if (color == 0)
                            sumBlackPoint++;
                    }
                }
            }
            return sumBlackPoint;
        }
    }
}