using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetHelper;
using ImageProcessor.ImageProcessing;
using Tesseract;

namespace ImageProcessor.Ocr
{
    public abstract class OcrEngine
    {
        public static bool Debug = false;
        public static bool Delete = true;

        public TesseractEngine Engine;

        protected const string TessdataBasePath = @"./tessdata/";
        

        public static bool DataFileAvailable(List<string> tessdataFilePaths)
        {
            return tessdataFilePaths.Select(Path.GetFullPath).All(path => ((FilePath) path).Exists()); 
        }
        
        protected OcrEngine()
        {
        }

        public void Dispose()
        {
            Engine.Dispose();
        }
        
        public Bitmap ExtendImage(Bitmap bmp)
        {
            var bitmap = new Bitmap((int)(bmp.Width * 1.1), bmp.Height, PixelFormat.Format32bppRgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
                graphics.DrawImage(bmp, (float)(0.05 * bitmap.Width), 0, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    GraphicsUnit.Pixel);
            }
            return bitmap;
        }

        public abstract OcrResult ProcessOcr(string path, HashSet<string> candidates);

        public abstract OcrResult ProcessOcr(double count, string path, HashSet<string> candidates);

        public static OcrEngine CreateEngine(OcrLanguage language)
        {
            switch (language)
            {
                case OcrLanguage.English:
                    return new OcrEngineEnglish();
                case OcrLanguage.SimplifiedChinese:
                    return new OcrEngineSimplifiedChinese();
                case OcrLanguage.TraditionalChinese:
                    return new OcrEngineTraditionalChinese();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsTessDataAvailable(OcrLanguage language)
        {
            switch (language)
            {
                case OcrLanguage.English:
                    return DataFileAvailable(OcrEngineEnglish.TessdataFilePaths);
                case OcrLanguage.SimplifiedChinese:
                    return DataFileAvailable(OcrEngineSimplifiedChinese.TessdataFilePaths);
                case OcrLanguage.TraditionalChinese:
                    return DataFileAvailable(OcrEngineTraditionalChinese.TessdataFilePaths);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string[] GetDirectory(OcrLanguage language)
        {
            switch (language)
            {
                case OcrLanguage.English:
                    return new [] {@"tessdata\enUS", @"get/tessdata/en-US" };
                case OcrLanguage.SimplifiedChinese:
                    return new [] { @"tessdata\zhCN", @"get/tessdata/zh-CN" };
                case OcrLanguage.TraditionalChinese:
                    return new [] { @"tessdata\zhTW", @"get/tessdata/zh-TW" };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static readonly HashSet<string> CandidateHeroes = new HashSet<string>();

        public static readonly HashSet<string> CandidateMaps = new HashSet<string>();

        public string PickingText { get; set; }
    }

    public class OcrResult
    {
        public OcrResult()
        {
            Results = new Dictionary<string, MatchResult>();
        }

        public Dictionary<string, MatchResult> Results { get; set; }

        public List<MatchResult> Values => Results.Select(r => r.Value).ToList();
    }

    public class MatchResult
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public bool InDoubt { get; set; }

        public int Score { get; set; }

        public bool Trustable { get; set; }

        public bool FullyTrustable { get; set; }
    }

    public enum OcrLanguage
    {
        English,
        SimplifiedChinese,
        TraditionalChinese,
        Unavailable
    }
}
