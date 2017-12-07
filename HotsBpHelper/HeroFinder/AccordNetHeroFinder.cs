using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Accord.Imaging;
using Accord.Imaging.Filters;
using HotsBpHelper.Utils;

namespace HotsBpHelper.HeroFinder
{
    public class AccordNetHeroFinder : IHeroFinder
    {
        private class TemplateInfo
        {
            public Bitmap Template { get; set; }
            public string FilePathName { get; set; }
            public string HeroName { get; set; }
        }

        private struct PositionInfo
        {
            /// <summary>
            /// 文件夹分类
            /// </summary>
            public string DirStr { get; set; }

            /// <summary>
            /// 英雄名称框矩形
            /// </summary>
            public Rectangle Rectangle { get; set; }

            /// <summary>
            /// 英雄名字框的剪切坐标
            /// </summary>
            public Point[] ClipPoints { get; set; }
        }

        private readonly IImageUtil _imageUtil;
        private const int MAX_HERO_IMAGES_COUNT = 5;
        private const string IMAGES_HEROES = @"Images\Heroes";
        private IDictionary<string, IDictionary<string, IList<TemplateInfo>>> _templatesDict;

        public AccordNetHeroFinder(IImageUtil imageUtil)
        {
            _imageUtil = imageUtil;
            LoadImages();
        }

        /// <summary>
        /// 从Images\Heroes中加载匹配模板
        /// </summary>
        private void LoadImages()
        {
            string[] dirStrs = { "left", "right", "ban" };
            string heroesDir = Path.Combine(App.AppPath, IMAGES_HEROES);
            string resolution = $"{App.MyPosition.Width}x{App.MyPosition.Height}";
            string leftDir = Path.Combine(heroesDir, resolution, "left");
            string rightDir = Path.Combine(heroesDir, resolution, "right");
            string banDir = Path.Combine(heroesDir, resolution, "ban");

            if (!Directory.Exists(heroesDir))
            {
                // 如果不存在英雄图片文件夹,则创建相应文件夹(第一次使用)
                Directory.CreateDirectory(leftDir);
                Directory.CreateDirectory(rightDir);
                Directory.CreateDirectory(banDir);
                return;
            }

            // 分别将各个文件夹下的图片,生成匹配用的模板,放到dict中
            _templatesDict = new Dictionary<string, IDictionary<string, IList<TemplateInfo>>>();
            foreach (var dirStr in dirStrs)
            {
                var heroTemplateDict = new Dictionary<string, IList<TemplateInfo>>();
                _templatesDict.Add(dirStr, heroTemplateDict);
                string dir = Path.Combine(heroesDir, resolution, dirStr);
                foreach (var bmpFile in Directory.EnumerateFiles(dir, "*.bmp").OrderByDescending(f => f))
                // 降序遍历文件,保证新模板在前
                {
                    // ReSharper disable once PossibleNullReferenceException
                    // bmpFile : 阿尔萨斯_20171205213930.bmp
                    string heroName = Path.GetFileNameWithoutExtension(bmpFile).Split('_')[0];
                    TemplateInfo ti;
                    using (var bmp = new Bitmap(bmpFile))
                    {
                        ti = new TemplateInfo
                        {
                            FilePathName = bmpFile,
                            Template = Grayscale.CommonAlgorithms.BT709.Apply(bmp),
                            HeroName = heroName,
                        };
                    }

                    if (!heroTemplateDict.ContainsKey(heroName))
                    {
                        heroTemplateDict.Add(heroName, new List<TemplateInfo>());
                    }
                    if (heroTemplateDict[heroName].Count == MAX_HERO_IMAGES_COUNT)
                    {
                        // 该英雄的模板数量已达上限, 则删除旧模板图片
                        File.Delete(bmpFile);
                        continue;
                    }
                    else
                    {
                        // 否则向缓存中加入匹配模板
                        heroTemplateDict[heroName].Add(ti);
                    }
                }
            }
        }

        /// <summary>
        /// 根据ID识别英雄
        /// </summary>
        /// <param name="id">BP用ID</param>
        /// <returns>英雄名字</returns>
        /// <remarks>
        ///  ID示意图
        ///    0 1   7 8
        ///  2          9
        ///  3          10
        ///  4          11
        ///  5          12
        ///  6          13
        /// 
        ///  </remarks>
        public string FindHero(int id)
        {
            var pi = CaculatePositionInfo(id);

            // TODO: ban处理
            if (pi.ClipPoints == null) return null;

            using (var screenBmp = _imageUtil.CaptureScreen())
            {
                using (var bmp = _imageUtil.CaptureArea(screenBmp, pi.Rectangle, pi.ClipPoints))
                {
                    var ti = GetMostSimpilarHeroTemplateInfo(bmp, pi.DirStr);
                    return ti?.HeroName;
                }
            }
        }

        private PositionInfo CaculatePositionInfo(int id)
        {
            PositionInfo result = new PositionInfo();
            int x, y;
            if (id == 0 || id == 1 || id == 7 || id == 8)
            {
                result.DirStr = "ban";
                result.ClipPoints = null; // TODO: 处理禁选
                result.Rectangle = Rectangle.Empty;
            }
            else if (id >= 2 && id <= 6)
            {
                result.DirStr = "left";
                result.ClipPoints = App.MyPosition.Left.HeroPathPoints;
                x = App.MyPosition.Left.HeroName1.X + (id % 2) * App.MyPosition.Left.Dx;
                y = App.MyPosition.Left.HeroName1.Y + (id - 2) * App.MyPosition.Left.Dy;
                if (id == 5 || id == 6)
                {
                    // 5和6需要再向下偏移一个像素
                    y++;
                }
                result.Rectangle = new Rectangle(x, y, App.MyPosition.HeroWidth, App.MyPosition.HeroHeight);
            }
            else
            {
                result.DirStr = "right";
                result.ClipPoints = App.MyPosition.Right.HeroPathPoints;
                x = App.MyPosition.Right.HeroName1.X + ((id + 1) % 2) * App.MyPosition.Right.Dx;
                y = App.MyPosition.Right.HeroName1.Y + (id - 9) * App.MyPosition.Right.Dy;
                if (id == 12 || id == 13)
                {
                    // 12和13需要再向下偏移一个像素
                    y++;
                }
                result.Rectangle = new Rectangle(x - App.MyPosition.HeroWidth, y, App.MyPosition.HeroWidth, App.MyPosition.HeroHeight);
            }
            return result;
        }

        public void AddNewTemplate(int id, string heroName)
        {
            var pi = CaculatePositionInfo(id);
            // TODO: ban处理
            if (pi.ClipPoints == null) return;

            string heroesDir = Path.Combine(App.AppPath, IMAGES_HEROES);
            string resolution = $"{App.MyPosition.Width}x{App.MyPosition.Height}";
            string imageFile = Path.Combine(heroesDir, resolution, pi.DirStr, $"{heroName}_{DateTime.Now:yyyyMMddhhmmss}.bmp");

            using (var screenBmp = _imageUtil.CaptureScreen())
            {
                using (var bmp = _imageUtil.CaptureArea(screenBmp, pi.Rectangle, pi.ClipPoints))
                {
                    bmp.Save(imageFile);
                    var dict = _templatesDict[pi.DirStr];
                    if (!dict.ContainsKey(heroName))
                    {
                        dict.Add(heroName, new List<TemplateInfo>());
                    }
                    dict[heroName].Add(new TemplateInfo
                    {
                        FilePathName = imageFile,
                        HeroName = heroName,
                        Template = Grayscale.CommonAlgorithms.BT709.Apply(bmp),
                    });
                }
            }
        }

        /// <summary>
        /// 在缓存中找到和指定图像最相似的图片的名字
        /// </summary>
        /// <param name="bmp">指定的图像</param>
        /// <param name="dirStr">在哪个文件夹中查找,取值:ban, left, right</param>
        /// <returns>最相似的TemplateInfo, 如未找到相似的则返回null</returns>
        private TemplateInfo GetMostSimpilarHeroTemplateInfo(Bitmap bmp, string dirStr)
        {
            using (var heroImage = Grayscale.CommonAlgorithms.BT709.Apply(bmp))
            {
                ExhaustiveTemplateMatching etm = new ExhaustiveTemplateMatching(0);

                float similarity = float.MinValue;
                TemplateInfo result = null;
                foreach (var template in _templatesDict[dirStr])
                {
                    foreach (var ti in template.Value)
                    {
                        if (ti.Template.Size.Width > heroImage.Size.Width || ti.Template.Size.Height > heroImage.Size.Height)
                            continue;
                        var tm = etm.ProcessImage(heroImage, ti.Template);
                        if (tm[0].Similarity > similarity && tm[0].Similarity > 0.99)
                        {
                            similarity = tm[0].Similarity;
                            result = ti;
                        }
                    }
                }
                return result;
            }
        }
    }
}