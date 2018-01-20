using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetHelper;
using ImageProcessor.ImageProcessing;
using Tesseract;

namespace ImageProcessor.Ocr
{
    public class OcrEngineEnglish : OcrEngineLatin
    {
        public OcrEngineEnglish()
        {
            Engine = new TesseractEngine(@".\tessdata\enUS", "eng", EngineMode.TesseractOnly);
            Engine.DefaultPageSegMode = PageSegMode.SingleLine;
            Engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ'.-");
            PickingText = "Picking";
            CandidateHeroes.Add(PickingText);
        }
    }

    public class OcrEngineLatin : OcrEngine
    {
        public Bitmap ExtendImage(Bitmap bmp)
        {
            Bitmap bitmap = new Bitmap((int) (bmp.Width * 1.1), bmp.Height, PixelFormat.Format32bppRgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
                graphics.DrawImage(bmp, (float) (0.05 * bitmap.Width), 0, new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            }
            return bitmap;
        }

        public override OcrResult ProcessOcr(string path, HashSet<string> candidates)
        {
            var ocrResult = new OcrResult();
            
            var inDoubt = false;
            FilePath filePath = path;
            using (var bitMap = new Bitmap(filePath))
            {
                using (var extendedBitmap = ExtendImage(bitMap))
                {
                    extendedBitmap.Save(filePath.GetDirPath() + @"tempExtended.bmp");
                }
            }

            using (Pix pix = Pix.LoadFromFile(filePath.GetDirPath() + @"tempExtended.bmp"))
            using (Page page = Engine.Process(pix))
            {
                string text = page.GetText();
                string match = string.Empty;
                double maxScore = 0;
                text = text.Replace(@"\n", string.Empty).Trim();
                foreach (string candidate in candidates)
                {
                    double score = 1 - ((double)DamerauLevenshteinDistance(text, candidate.ToUpper(), candidate.Length / 2)) /
                                   candidate.Length;
                    if (score < 0)
                        continue;

                    if (maxScore < score)
                    {
                        maxScore = score;
                        match = candidate;
                    }
                }
                ocrResult.Results[match] = new MatchResult
                {
                    Key = text,
                    Value = match,
                    InDoubt = maxScore < 0.9,
                    Score = (int)(maxScore * 100),
                    Trustable = maxScore > 0.9,
                    FullyTruestable = maxScore > 0.95
                };
                return ocrResult;
            }
        }

        public override OcrResult ProcessOcr(double count, string path, HashSet<string> candidates)
        {
            return ProcessOcr(path, candidates);
        }

        static void Swap<T>(ref T arg1, ref T arg2)
        {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }

        /// <summary>
        /// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
        /// integers, where each integer represents the code point of a character in the source string.
        /// Includes an optional threshhold which can be used to indicate the maximum allowable distance.
        /// </summary>
        /// <param name="source">An array of the code points of the first string</param>
        /// <param name="target">An array of the code points of the second string</param>
        /// <param name="threshold">Maximum allowable distance</param>
        /// <returns>Int.MaxValue if threshhold exceeded; otherwise the Damerau-Leveshteim distance between the strings</returns>
        public static int DamerauLevenshteinDistance(string source, string target, int threshold)
        {

            int length1 = source.Length;
            int length2 = target.Length;

            // Return trivial case - difference in string lengths exceeds threshhold
            if (Math.Abs(length1 - length2) > threshold)
            {
                return int.MaxValue;
            }

            // Ensure arrays [i] / length1 use shorter length 
            if (length1 > length2)
            {
                Swap(ref target, ref source);
                Swap(ref length1, ref length2);
            }

            int maxi = length1;
            int maxj = length2;

            int[] dCurrent = new int[maxi + 1];
            int[] dMinus1 = new int[maxi + 1];
            int[] dMinus2 = new int[maxi + 1];
            int[] dSwap;

            for (int i = 0; i <= maxi; i++)
            {
                dCurrent[i] = i;
            }

            int jm1 = 0, im1 = 0, im2 = -1;

            for (int j = 1; j <= maxj; j++)
            {

                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                int minDistance = int.MaxValue;
                dCurrent[0] = j;
                im1 = 0;
                im2 = -1;

                for (int i = 1; i <= maxi; i++)
                {

                    int cost = source[im1] == target[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                    if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                        min = Math.Min(min, dMinus2[im2] + cost);

                    dCurrent[i] = min;
                    if (min < minDistance)
                    {
                        minDistance = min;
                    }
                    im1++;
                    im2++;
                }
                jm1++;
                if (minDistance > threshold)
                {
                    return int.MaxValue;
                }
            }

            int result = dCurrent[maxi];
            return (result > threshold) ? int.MaxValue : result;
        }

        public static List<string> Candidates = new List<string>
        {
            "Picking",
            "Zeratul",
            "Valla",
            "Uther",
            "Tyrande",
            "Tyrael",
            "Tassadar",
            "Stitches",
            "Sonya",
            "Sgt. Hammer",
            "Raynor",
            "Nova",
            "Nazeebo",
            "Muradin",
            "Malfurion",
            "Kerrigan",
            "Illidan",
            "Gazlowe",
            "Falstad",
            "E.T.C.",
            "Diablo",
            "Arthas",
            "Abathur",
            "Tychus",
            "Li Li",
            "Brightwing",
            "Murky",
            "Zagara",
            "Rehgar",
            "Chen",
            "Azmodan",
            "Anub'arak",
            "Jaina",
            "Thrall",
            "The Lost Vikings",
            "Sylvanas",
            "Kael'thas",
            "Johanna",
            "The Butcher",
            "Leoric",
            "Kharazim",
            "Rexxar",
            "Lt. Morales",
            "Artanis",
            "Cho",
            "Gall",
            "Lunara",
            "Greymane",
            "Li-Ming",
            "Xul",
            "Dehaka",
            "Tracer",
            "Chromie",
            "Medivh",
            "Gul'dan",
            "Auriel",
            "Alarak",
            "Zarya",
            "Samuro",
            "Varian",
            "Ragnaros",
            "Zul'jin",
            "Valeera",
            "Lúcio",
            "Probius",
            "Cassia",
            "Genji",
            "D.Va",
            "Malthael",
            "Stukov",
            "Garrosh",
            "Kel'Thuzad",
            "Ana",
            "Junkrat",
            "Alexstrasza"
            ,
            "Hanzo",
            "Blaze"
        };
    }

    
}
