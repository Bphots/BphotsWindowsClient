using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ImageProcessor.ImageProcessing;
using Tesseract;

namespace ImageProcessor.Ocr
{
    public abstract class OcrEngine
    {
        public static bool Debug = false;

        protected TesseractEngine Engine;

        protected OcrEngine()
        {
        }

        public void Dispose()
        {
            Engine.Dispose();
        }

        public abstract MatchResult ProcessOcr(string path, HashSet<string> candidates);

        public abstract MatchResult ProcessOcr(double count, string path, HashSet<string> candidates);

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

        public static readonly HashSet<string> CandidateHeroes = new HashSet<string>();

        public static readonly HashSet<string> CandidateMaps = new HashSet<string>();

        public string PickingText { get; set; }
    }

    public enum OcrLanguage
    {
        English,
        SimplifiedChinese,
        TraditionalChinese,
        Korean
    }
}
