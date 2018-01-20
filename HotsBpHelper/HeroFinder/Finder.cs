using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DotNetHelper;
using HotsBpHelper.Utils;
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

        public void AddNewTemplate(int id, string heroName, Dictionary<int, string> fileDictionary)
        {
            var imageUtil = new ImageUtils();
            var positionInfo = CalculatePositionInfo(id);

            if (positionInfo.ClipPoints == null)
                return;

            var path = Path.Combine(App.AppPath, "Images\\Heroes");
            var path2 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
            FilePath screenshotPath = Path.Combine(path, path2, "Screenshot.png");
            FilePath text = Path.Combine(path, path2, positionInfo.DirStr,
                string.Format("{0}_{1:yyyyMMddhhmmss}.bmp", heroName, DateTime.Now));
            if (!text.GetDirPath().Exists)
                Directory.CreateDirectory(text.GetDirPath());

            using (var bitmap = imageUtil.CaptureScreen())
            using (var bitmap2 = imageUtil.CaptureArea(bitmap, positionInfo.Rectangle, positionInfo.ClipPoints))
            {
                bitmap2.Save(text);
                fileDictionary[id] = text;
            }
        }

        private PositionInfo CalculatePositionInfo(int id)
        {
            var result = default(PositionInfo);
            var flag = id == 0 || id == 1 || id == 7 || id == 8;
            if (flag)
            {
                result.DirStr = "ban";
                result.ClipPoints = null;
                result.Rectangle = Rectangle.Empty;
            }
            else
            {
                var flag2 = id >= 2 && id <= 6;
                if (flag2)
                {
                    result.DirStr = "left";
                    result.ClipPoints = App.AppSetting.Position.Left.HeroPathPoints;
                    var num = App.AppSetting.Position.Left.HeroName1.X + id%2*App.AppSetting.Position.Left.Dx;
                    var num2 = App.AppSetting.Position.Left.HeroName1.Y + (id - 2)*App.AppSetting.Position.Left.Dy;
                    var flag3 = id == 5 || id == 6;
                    if (flag3)
                        num2++;
                    result.Rectangle = new Rectangle(num, num2, App.AppSetting.Position.HeroWidth, App.AppSetting.Position.HeroHeight);
                }
                else
                {
                    result.DirStr = "right";
                    result.ClipPoints = App.AppSetting.Position.Right.HeroPathPoints;
                    var num = App.AppSetting.Position.Right.HeroName1.X + (id + 1)%2*App.AppSetting.Position.Right.Dx;
                    var num2 = App.AppSetting.Position.Right.HeroName1.Y + (id - 9)*App.AppSetting.Position.Right.Dy;
                    var flag4 = id == 12 || id == 13;
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