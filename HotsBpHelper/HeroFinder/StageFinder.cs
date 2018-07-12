using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using HotsBpHelper.Utils;
using ImageProcessor.Extensions;
using ToastNotifications.Lifetime;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace HotsBpHelper.HeroFinder
{
    public static class StageFinder
    {
        private static int GetMaxIndex(string sequence)
        {
            var intList = sequence.Select(c => int.Parse(c.ToString())).ToList();
            var maxIndex = -1;
            var maxValue = 45;
            for (int i = 0; i < 12; ++i)
            {
                var sum = intList.Skip(i).Take(11).Sum();
                if (sum > maxValue)
                {
                    maxValue = sum;
                    maxIndex = i;
                }
            }

            return maxIndex;
        }
        
        public static StageInfo ProcessStageInfo(Bitmap screenshotBitmap)
        {
            var circleBitmaps = new List<Bitmap>();
            var circles = new List<FastBitmap>();
            try
            {
                InitializeCircles(circleBitmaps, circles, screenshotBitmap);
                var sb = new StringBuilder();
                
                for (var i = 0; i < 22; i++)
                {
                    var bitmap = circleBitmaps[i];
                    var samples = GetCentrePixelSamples(bitmap);
                    var regular = IsColorConsistent(samples, 25);
                    var matchingBorders = MatchingBorders(bitmap);

                    var score = 0;
                    if (regular)
                    {
                        if (matchingBorders >= 3)
                            score = matchingBorders + 1;
                        else
                            score = 0;
                    }
                    else
                    {
                        if (matchingBorders >= 3)
                            score = matchingBorders;
                        else
                            score = 0;
                    }

                    sb.Append(score.ToString());
                }

                var tempCheckString = sb.ToString();
                var beginInex = GetMaxIndex(tempCheckString);

                if (beginInex == -1)
                    return new StageInfo { Step = -1, IsFirstPick = false, Error = tempCheckString };

                var lastBitmap = circleBitmaps[beginInex + 10];
                var circle9R = lastBitmap.GetPixel(lastBitmap.Width / 2, lastBitmap.Height / 2).R;

                var step = 11 - beginInex;
                var isFirstPick = circle9R > 45;
                if (step == 11)
                    return new StageInfo { Step = step, IsFirstPick = !isFirstPick, Error = tempCheckString };

                return new StageInfo {Step = step, IsFirstPick = isFirstPick, Error = tempCheckString };
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                circles.ForEach(c => c.Dispose());
                circleBitmaps.ForEach(c => c.Dispose());
            }
            return new StageInfo {Step = -1, IsFirstPick = false};
        }
        
        private static List<Color> GetCentrePixelSamples(Bitmap bitmap)
        {
            List<Color> colors = new List<Color>();
            var sampleDistance = bitmap.Width < 20 ? 1 : 2;
            for (int x = bitmap.Width/2 - sampleDistance; x <= bitmap.Width / 2 + sampleDistance; ++x)
                for (int y = bitmap.Height/2 - sampleDistance; y <= bitmap.Height/2 + sampleDistance; ++y)
                {
                    colors.Add(bitmap.GetPixel(x, y));
                }
            return colors;
        }

        private static int MatchingBorders(Bitmap bitmap)
        {
            int matchingBorders = 0;
            Color sampleColor = Color.Empty;
            var topColors = new List<Color>();
            bool topBorderDetected = false;
            for (int y = 0; y <= bitmap.Height/5; ++y)
            {
                var color = bitmap.GetPixel(bitmap.Width / 2, y);
                topColors.Add(color);
                if (sampleColor == Color.Empty)
                {
                    sampleColor = color;
                    continue;
                }
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) > 30)
                {
                    topBorderDetected = true;
                    break;
                }
                sampleColor = bitmap.GetPixel(bitmap.Width / 2, y);
            }
            if (topBorderDetected)
                matchingBorders++;

            sampleColor = Color.Empty;
            var bottomColors = new List<Color>();
            bool bottomBorderDetected = false;
            for (int y = bitmap.Height - 1; y >= bitmap.Height - 1 - bitmap.Height / 5; --y)
            {
                var color = bitmap.GetPixel(bitmap.Width / 2, y);
                bottomColors.Add(color);
                if (sampleColor == Color.Empty)
                {
                    sampleColor = color;
                    continue;
                }
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) > 30)
                {
                    bottomBorderDetected = true;
                    break;
                }
                sampleColor = bitmap.GetPixel(bitmap.Width / 2, y);
            }
            if (bottomBorderDetected)
                matchingBorders++;
            if (matchingBorders < 2)
                return 0;

            sampleColor = Color.Empty;
            var leftColors = new List<Color>();
            bool leftBorderDetected = false;
            for (int x = 0; x <= bitmap.Width / 5; ++x)
            {
                var color = bitmap.GetPixel(x, bitmap.Height / 2);
                leftColors.Add(color);
                if (sampleColor == Color.Empty)
                {
                    sampleColor = color;
                    continue;
                }
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) > 30)
                {
                    leftBorderDetected = true;
                    break;
                }
                sampleColor = bitmap.GetPixel(x, bitmap.Height / 2);
            }
            if (leftBorderDetected)
                matchingBorders++;
            if (matchingBorders < 2)
                return 0;

            sampleColor = Color.Empty;
            var rightColors = new List<Color>();
            bool rightBorderDetected = false;
            for (int x = bitmap.Width - 1; x >= bitmap.Width - 1 - bitmap.Width / 5; --x)
            {
                var color = bitmap.GetPixel(x, bitmap.Height / 2);
                rightColors.Add(color);
                if (sampleColor == Color.Empty)
                {
                    sampleColor = bitmap.GetPixel(x, bitmap.Height / 2);
                    continue;
                }
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) > 30)
                {
                    rightBorderDetected = true;
                    break;
                }
                sampleColor = bitmap.GetPixel(x, bitmap.Height / 2);
            }
            if (rightBorderDetected)
                matchingBorders++;

            if (matchingBorders < 3)
                return 0;

            return matchingBorders;
        }

        private static bool IsColorConsistent(List<Color> colors, int tolarance)
        {
            if (!colors.Any())
                return false;

            var sampleColor = colors[colors.Count / 2];
            int faultCount = 0;
            foreach (var color in colors)
            {
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) + Math.Abs(color.B - sampleColor.B) > tolarance)
                {
                    faultCount++;
                    if (faultCount > 0)
                        return false;
                }
            }
            
            return true;
        }
        
        private static void InitializeCircles(List<Bitmap> circleBitmaps, List<FastBitmap> circles,
            Bitmap screenshotBitmap)
        {
            //float dpiX, dpiY;
            //using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            //{
            //    dpiX = graphics.DpiX;
            //    dpiY = graphics.DpiY;
            //}
            //screenshotBitmap.SetResolution(dpiX, dpiY);
            int width = screenshotBitmap.Width;
            int height = screenshotBitmap.Height;
            var imageUtil = new ImageUtils();
            var listCirclePosition = new List<PositionInfo>();
            for (var i = 11; i >= 1; --i)
            {
                listCirclePosition.Add(GetLeftCirclePositionInfo(i, width, height));
            }
            for (var i = 1; i <= 11; ++i)
            {
                listCirclePosition.Add(GetRightCirclePositionInfo(i, width, height));
            }
            
            circleBitmaps.AddRange(
                listCirclePosition.Select(
                    positionInfo => imageUtil.CaptureArea(screenshotBitmap, positionInfo.Rectangle, positionInfo.ClipPoints)));
        }

        private static PositionInfo GetRightCirclePositionInfo(int index, int width, int height)
        {
            var positionInfo = new PositionInfo
            {
                Rectangle =
                    new Rectangle(
                        new Point(
                            (int)
                                Math.Round(0.5* width + 0.05903 * height +
                                 (index - 1)* 0.01909722222222222222222222222222 * height, 0),
                            (int)Math.Round(0.08025* height, 0)),
                        new Size((int)Math.Round(0.0174* height, 0), (int)Math.Round(0.0174* height, 0))),
                ClipPoints = new Point[0]
            };
            return positionInfo;
        }

        private static PositionInfo GetLeftCirclePositionInfo(int index, int width, int height)
        {
            var positionInfo = new PositionInfo
            {
                Rectangle =
                    new Rectangle(
                        new Point(
                            (int)
                                Math.Round(0.5* width - 0.0590 * height -
                                 (index - 1)* 0.01909722222222222222222222222222 * height - 0.0174* height, 0),
                            (int)Math.Round(0.08025* height, 0)),
                        new Size((int)Math.Round(0.0174* height, 0), (int)Math.Round(0.0174* height, 0))),
                ClipPoints = new Point[0]
            };
            return positionInfo;
        }
    }

    public class StageInfo
    {
        public StageInfo()
        {
            Step = -1;
        }

        public int Step { get; set; }

        public bool IsFirstPick { get; set; }

        public string Error { get; set; }
    }
}