using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DotNetHelper;
using HotsBpHelper.Utils;
using ImageProcessor.HashProcessing;
using ImageProcessor.ImageProcessing;

namespace HotsBpHelper.HeroFinder
{
    public class Finder
    {
        public StageInfo GetStageInfo()
        {
            lock (ImageProcessingHelper.GDILock)
            {
                var imageUtil = new ImageUtils();
                using (var bitmap = imageUtil.CaptureScreen())
                {
                    return StageFinder.ProcessStageInfo(bitmap);
                }
            }
        }

        public Tuple<int, double> GetBanHero(int index)
        {
            lock (ImageProcessingHelper.GDILock)
            {
                var imageUtil = new ImageUtils();
                var path = Path.Combine(App.AppPath, "Images\\Heroes");
                var path2 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
                FilePath text = Path.Combine(path, path2, "Ban",
                    string.Format("{0}_{1:yyyyMMddhhmmss}.jpg", index, DateTime.Now));

                if (!text.GetDirPath().Exists)
                    Directory.CreateDirectory(text.GetDirPath());

                using (var bitmap = imageUtil.CaptureBanArea(App.AppSetting.Position.BanPositions[index]))
                {
                    bitmap.Save(text);
                }
                Tuple<int, double> result = null;
                using (var bitmap = new Bitmap(text))
                {
                    var path1 = Path.Combine(App.AppPath, "Images\\Heroes");
                    var path12 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
                    DirPath text2 = Path.Combine(path1, path12, "Ban\\Out");
                    if (!text2.Exists)
                        Directory.CreateDirectory(text2);

                    result = HeroIdentifier.Identify(bitmap);
                    FilePath resultFilePath = Path.Combine(text2, result.Item1 + " " + result.Item2 + ".bmp");
                    if (App.Debug)
                        bitmap.Save(resultFilePath);
                }

                if (!App.Debug)
                    text.DeleteIfExists();

                return result;
            }
        }
        
        public FilePath CaptureScreen()
        {
            var imageUtil = new ImageUtils();
            var path = Path.Combine(App.AppPath, "Images\\Heroes");
            var path2 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
            FilePath text = Path.Combine(path, path2, "Screenshot.png");
            if (!text.GetDirPath().Exists)
                Directory.CreateDirectory(text.GetDirPath());

            using (var bitmap = imageUtil.CaptureScreen())
                bitmap.Save(text);
            return text;
        }

        public string CaptureMapArea(FilePath screenFilePath)
        {
            var imageUtil = new ImageUtils();
            var positionInfo = new PositionInfo
            {
                Rectangle =
                    new Rectangle(App.AppSetting.Position.MapPosition.Location,
                        new Size(App.AppSetting.Position.MapPosition.Width, App.AppSetting.Position.MapPosition.Height)),
                ClipPoints = new Point[0]
            };
            var path = Path.Combine(App.AppPath, "Images\\Heroes");
            var path2 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
            FilePath text = Path.Combine(path, path2, "map.bmp");
            if (!text.GetDirPath().Exists)
                Directory.CreateDirectory(text.GetDirPath());

            using (var bitmap = new Bitmap(screenFilePath))
            using (var bitmap2 = imageUtil.CaptureArea(bitmap, positionInfo.Rectangle, positionInfo.ClipPoints))
            {
                bitmap2.Save(text);
            }
            return text;
        }

        public string CaptureLeftLoadingLabel(Bitmap bitmap, int index)
        {
            var imageUtil = new ImageUtils();

            int left = App.AppSetting.Position.LoadingPoints.LeftFirstPoint.X;
            int top = App.AppSetting.Position.LoadingPoints.LeftFirstPoint.Y +
                      index*App.AppSetting.Position.LoadingPoints.Dy;


            var positionInfo = new PositionInfo
            {
                Rectangle =
                    new Rectangle(new Point(left, top), 
                        new Size(App.AppSetting.Position.LoadingPoints.Width, App.AppSetting.Position.LoadingPoints.Height)),
                ClipPoints = new Point[0]
            };
            var path = Path.Combine(App.AppPath, "Images\\Heroes");
            var path2 = $"{App.AppSetting.Position.Width}x{App.AppSetting.Position.Height}";
            FilePath text = Path.Combine(path, path2, "LoadingLeft" + index + ".bmp");
            if (!text.GetDirPath().Exists)
                Directory.CreateDirectory(text.GetDirPath());
            
            using (var bitmap2 = imageUtil.CaptureArea(bitmap, positionInfo.Rectangle, positionInfo.ClipPoints))
            {
                bitmap2.Save(text);
            }
            return text;
        }

        public string CaptureRightLoadingLabel(Bitmap bitmap, int index)
        {
            var imageUtil = new ImageUtils();
            int left = App.AppSetting.Position.LoadingPoints.RightFirstPoint.X;
            int top = App.AppSetting.Position.LoadingPoints.RightFirstPoint.Y +
                      index * App.AppSetting.Position.LoadingPoints.Dy;

            var positionInfo = new PositionInfo
            {
                Rectangle =
                   new Rectangle(new Point(left, top),
                        new Size(App.AppSetting.Position.LoadingPoints.Width, App.AppSetting.Position.LoadingPoints.Height)),
                ClipPoints = new Point[0]
            };
            var path = Path.Combine(App.AppPath, "Images\\Heroes");
            var path2 = $"{App.AppSetting.Position.Width}x{App.AppSetting.Position.Height}";
            FilePath text = Path.Combine(path, path2, "LoadingRight" + index + ".bmp");
            if (!text.GetDirPath().Exists)
                Directory.CreateDirectory(text.GetDirPath());

            using (var bitmap2 = imageUtil.CaptureArea(bitmap, positionInfo.Rectangle, positionInfo.ClipPoints))
            {
                bitmap2.Save(text);
            }
            return text;
        }

        public void AddNewTemplate(int id, string heroName, Dictionary<int, string> fileDictionary, FilePath screenshotPath)
        {
            var imageUtil = new ImageUtils();
            var positionInfo = CalculatePositionInfo(id);

            if (positionInfo.ClipPoints == null)
                return;

            var path = Path.Combine(App.AppPath, "Images\\Heroes");
            var path2 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
            FilePath text = Path.Combine(path, path2, positionInfo.DirStr,
                string.Format("{0}_{1:yyyyMMddhhmmss}.bmp", heroName, DateTime.Now));
            if (!text.GetDirPath().Exists)
                Directory.CreateDirectory(text.GetDirPath());

            using (var bitmap = new Bitmap(screenshotPath))
            using (var bitmap2 = imageUtil.CaptureArea(bitmap, positionInfo.Rectangle, positionInfo.ClipPoints))
            {
                bitmap2.Save(text);
                fileDictionary[id] = text;
            }
        }

        public Bitmap AddNewTemplateBitmap(int id, FilePath screenshotPath)
        {
            using (var bitmap = new Bitmap(screenshotPath))
                return AddNewTemplateBitmap(id, bitmap);
        }

        public void AddNewTemplate(int id, string heroName, Dictionary<int, string> fileDictionary, Bitmap screenshotBitmap)
        {
            var imageUtil = new ImageUtils();
            var positionInfo = CalculatePositionInfo(id);

            if (positionInfo.ClipPoints == null)
                return;

            var path = Path.Combine(App.AppPath, "Images\\Heroes");
            var path2 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
            FilePath text = Path.Combine(path, path2, positionInfo.DirStr,
                string.Format("{0}_{1:yyyyMMddhhmmss}.bmp", heroName, DateTime.Now));
            if (!text.GetDirPath().Exists)
                Directory.CreateDirectory(text.GetDirPath());
            
            using (var bitmap2 = imageUtil.CaptureArea(screenshotBitmap, positionInfo.Rectangle, positionInfo.ClipPoints))
            {
                bitmap2.Save(text);
                fileDictionary[id] = text;
            }
        }

        public Bitmap AddNewTemplateBitmap(int id, Bitmap screenshotBitmap)
        {
            var imageUtil = new ImageUtils();
            var positionInfo = CalculatePositionInfo(id);

            if (positionInfo.ClipPoints == null)
                return null;

            var path = Path.Combine(App.AppPath, "Images\\Heroes");
            var path2 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
            FilePath text = Path.Combine(path, path2, positionInfo.DirStr,
                $"{DateTime.Now:yyyyMMddhhmmss}.bmp");

            if (!text.GetDirPath().Exists)
                Directory.CreateDirectory(text.GetDirPath());

            var bitmap2 = imageUtil.CaptureArea(screenshotBitmap, positionInfo.Rectangle, positionInfo.ClipPoints);

            return bitmap2;
        }


        public Bitmap AddNewTemplate(int id, bool textInWhite = false)
        {
            var imageUtil = new ImageUtils();
            var positionInfo = CalculatePositionInfo(id);

            if (positionInfo.ClipPoints == null)
                return null;

            var path = Path.Combine(App.AppPath, "Images\\Heroes");
            var path2 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
            FilePath text = Path.Combine(path, path2, positionInfo.DirStr,
                $"{DateTime.Now:yyyyMMddhhmmss}.bmp");
            if (!text.GetDirPath().Exists)
                Directory.CreateDirectory(text.GetDirPath());

            using (var bitmap = imageUtil.CaptureScreen())
            {
                var bitmap2 = imageUtil.CaptureArea(bitmap, positionInfo.Rectangle, positionInfo.ClipPoints, textInWhite);
                return bitmap2;
            }
        }

        private PositionInfo CalculatePositionInfo(int id)
        {
            var result = default(PositionInfo);
            var flag = id == 0 || id == 1 || id == 2 || id == 8 || id == 9 || id == 10;
            if (flag)
            {
                result.DirStr = "ban";
                result.ClipPoints = null;
                result.Rectangle = Rectangle.Empty;
            }
            else
            {
                var flag2 = id >= 3 && id <= 7;
                if (flag2)
                {
                    result.DirStr = "left";
                    result.ClipPoints = App.AppSetting.Position.Left.HeroPathPoints;
                    var num = App.AppSetting.Position.Left.HeroName1.X + (id + 1) % 2 * App.AppSetting.Position.Left.Dx;
                    var num2 = App.AppSetting.Position.Left.HeroName1.Y + (id - 3)*App.AppSetting.Position.Left.Dy;
                    var flag3 = id == 6 || id == 7;
                    if (flag3)
                        num2++;
                    result.Rectangle = new Rectangle(num, num2, App.AppSetting.Position.HeroWidth, App.AppSetting.Position.HeroHeight);
                }
                else
                {
                    result.DirStr = "right";
                    result.ClipPoints = App.AppSetting.Position.Right.HeroPathPoints;
                    var num = App.AppSetting.Position.Right.HeroName1.X + (id + 1)%2*App.AppSetting.Position.Right.Dx;
                    var num2 = App.AppSetting.Position.Right.HeroName1.Y + (id - 11)*App.AppSetting.Position.Right.Dy;
                    var flag4 = id == 14 || id == 15;
                    if (flag4)
                        num2++;
                    result.Rectangle = new Rectangle(num - App.AppSetting.Position.HeroWidth, num2, App.AppSetting.Position.HeroWidth,
                        App.AppSetting.Position.HeroHeight);
                }
            }
            return result;
        }

    }


    public struct PositionInfo
    {
        // Token: 0x1700006D RID: 109
        // (get) Token: 0x060001DA RID: 474 RVA: 0x00007CEF File Offset: 0x00005EEF
        // (set) Token: 0x060001DB RID: 475 RVA: 0x00007CF7 File Offset: 0x00005EF7
        public string DirStr { get; set; }

        // Token: 0x1700006E RID: 110
        // (get) Token: 0x060001DC RID: 476 RVA: 0x00007D00 File Offset: 0x00005F00
        // (set) Token: 0x060001DD RID: 477 RVA: 0x00007D08 File Offset: 0x00005F08
        public Rectangle Rectangle { get; set; }

        // Token: 0x1700006F RID: 111
        // (get) Token: 0x060001DE RID: 478 RVA: 0x00007D11 File Offset: 0x00005F11
        // (set) Token: 0x060001DF RID: 479 RVA: 0x00007D19 File Offset: 0x00005F19
        public Point[] ClipPoints { get; set; }
    }
}