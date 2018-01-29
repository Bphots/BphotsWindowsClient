using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotNetHelper;
using ImageProcessor.ImageProcessing;
using ImageProcessor.Ocr;

namespace ImageProcessor
{
    public class Recognizer : IDisposable
    {
        private const int DarkModeThreshold = 60;
        private const int LightModeThreshold = 115;

        private readonly OcrEngine _engine;

        public static DirPath TempDirectoryPath;

        private bool m_isDarkMode;

        public string PickingText => _engine.PickingText;


        public Recognizer(OcrLanguage language, string sourceDirectory = null)
        {
            _engine = OcrEngine.CreateEngine(language);
            
            TempDirectoryPath = sourceDirectory + @"\Test\";
            if (!Directory.Exists(TempDirectoryPath))
                Directory.CreateDirectory(TempDirectoryPath);
        }

        public void Dispose()
        {
            _engine.Dispose();
        }

        /// <summary>
        ///     Processing the file with a rotating angle
        /// </summary>
        /// <param name="file"></param>
        /// <param name="angle"></param>
        /// <param name="sb"></param>
        public bool Recognize(string file, float angle, StringBuilder sb, int offset)
        {
            return ProcessHero(file, angle, sb, offset);
        }

        public void ProcessMap(FilePath file, StringBuilder sb)
        {
            var tempPath = TempDirectoryPath + "temp.tiff";
            using (var image = ImageProcessingHelper.GetCroppedMap(file))
                image.Save(tempPath);
            
            var pendingMatchResult = _engine.ProcessOcr(tempPath, OcrEngine.CandidateMaps);
            if (!pendingMatchResult.Results.Any() || !pendingMatchResult.Values.First().Trustable)
            {
                ResetFlags();
                return;
            }

            var i = 0;
            var path = TempDirectoryPath + pendingMatchResult.Values.First().Value + ".tiff";
            while (File.Exists(path))
            {
                ++i;
                path = TempDirectoryPath + pendingMatchResult.Values.First().Value + i + ".tiff";
            }

            sb.Append(pendingMatchResult.Values.First().Value);

            File.Move(file, path);
            ResetFlags();
        }

        private bool ProcessHero(FilePath file, float rotationAngle, StringBuilder sb, int offset)
        {
            var tempPath = TempDirectoryPath + "temp.tiff";
            var mode = ImageProcessingHelper.CheckMode(file, rotationAngle);
            if (mode == -1)
                return false;

            m_isDarkMode = mode == 0;
            var startThresholding = m_isDarkMode ? DarkModeThreshold : LightModeThreshold;

            int sampleWidth;
            var image = ImageProcessingHelper.GetCroppedImage(rotationAngle, file, m_isDarkMode, out sampleWidth);
            if (OcrEngine.Debug)
                image.Save(TempDirectoryPath + "CroppedImage.bmp");

            var count = (double)image.Width / sampleWidth * 7.5 + 0.1;

            if (_engine is OcrEngineAsian)
                _engine.Engine.SetVariable(@"textord_min_xheight", 25);

            string pendingMatchResult = string.Empty;

            var scoreDictionary = new Dictionary<string, int>();

            // 130 - 135 - 125 - 140 - 120 - 145 - 115
            // 75 - 80 - 70 - 85 - 65 - 90 - 60
            var switcher = 0;
            int faultCount = 0;
            for (var index = startThresholding + 15; index <= startThresholding + 30; index += switcher)
            {
                switcher = -switcher;
                if (switcher > 0)
                    switcher += offset;
                else
                    switcher -= offset;

                var segmentationCount = ImageProcessingHelper.ProcessOnce(index, image, tempPath);
                var newCount = count;
                if (segmentationCount < count && count - segmentationCount >= 2)
                    newCount = segmentationCount;

                var result = _engine.ProcessOcr(newCount, tempPath,
                        OcrEngine.CandidateHeroes);

                // 100% match case
                if (result.Values.Any(v => v.FullyTrustable))
                {
                    scoreDictionary[result.Values.First(v => v.FullyTrustable).Value] = int.MaxValue;
                    break;
                }

                // emptry case
                if (!result.Values.Any())
                {
                    faultCount++;
                    if (faultCount > 3)
                        break;

                    continue;
                }

                var maxScoreInSuite = result.Values.Max(c => c.Score);
                var matchResultsWithMaxScore = result.Values.Where(c => c.Score == maxScoreInSuite).ToList();

                // unique 60%+ case
                if (matchResultsWithMaxScore.Count == 1 && matchResultsWithMaxScore[0].Trustable)
                {
                    scoreDictionary[matchResultsWithMaxScore[0].Value] = int.MaxValue / 2;
                    break;
                }

                // normal case
                foreach (var matchResultWithMaxScore in matchResultsWithMaxScore)
                {
                    if (scoreDictionary.ContainsKey(matchResultWithMaxScore.Value))
                        scoreDictionary[matchResultWithMaxScore.Value] += matchResultWithMaxScore.Score;
                    else
                        scoreDictionary[matchResultWithMaxScore.Value] = matchResultWithMaxScore.Score;

                    if (OcrEngine.Debug)
                        Console.WriteLine(@"Thresdhold " + index + @" : " + matchResultWithMaxScore.Key.Replace("\n", string.Empty) + @" => " + matchResultWithMaxScore.Value);
                }
            }

            int maxValue = 0;
            foreach (var scorePair in scoreDictionary)
            {
                if (scorePair.Value > maxValue)
                {
                    pendingMatchResult = scorePair.Key;
                    maxValue = scorePair.Value;
                }
            }

            if (OcrEngine.Debug)
            {
                var i = 0;
                var path = TempDirectoryPath + pendingMatchResult + ".tiff";
                while (File.Exists(path))
                {
                    ++i;
                    path = TempDirectoryPath + pendingMatchResult + i + ".tiff";
                }

                if (!string.IsNullOrEmpty(sb.ToString()) || sb.ToString() != PickingText)
                    File.Copy(file, path, true);
            }


            sb.Append(pendingMatchResult);
            image.Dispose();

          
            if (!OcrEngine.Debug)
                file.DeleteIfExists();

            ResetFlags();

            return maxValue == int.MaxValue;
        }

        private void ResetFlags()
        {
            m_isDarkMode = false;
        }
    }
}