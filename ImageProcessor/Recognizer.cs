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
        
        public string PickingText => _engine.PickingText;


        public Recognizer(OcrLanguage language, string sourceDirectory = null)
        {
            _engine = OcrEngine.CreateEngine(language);
            
            TempDirectoryPath = sourceDirectory + @"\Test\";
            if (!Directory.Exists(TempDirectoryPath))
                Directory.CreateDirectory(TempDirectoryPath);
            var direLeft = TempDirectoryPath + @"left\";
            if (!Directory.Exists(direLeft))
                Directory.CreateDirectory(direLeft);
            var direRight = TempDirectoryPath + @"right\";
            if (!Directory.Exists(direRight))
                Directory.CreateDirectory(direRight);
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
        /// <param name="textInWhite"></param>
        public bool Recognize(string file, float angle, StringBuilder sb, int offset, bool textInWhite = false)
        {
            return ProcessHero(file, angle, sb, offset, textInWhite);
        }

        public bool ProcessMap(FilePath file, StringBuilder sb)
        {
            var tempPath = TempDirectoryPath + "temp.tiff";
            using (var image = ImageProcessingHelper.GetCroppedMap(file))
                image.Save(tempPath);
            
            OcrResult pendingMatchResult;

            try
            {
                pendingMatchResult = _engine.ProcessOcr(tempPath, OcrEngine.CandidateMaps);
            }
            catch (Exception)
            {
                return false;
            }

            if (!pendingMatchResult.Results.Any() || !pendingMatchResult.Values.First().Trustable)
            {
                return false;
            }

            if (OcrEngine.Debug)
            {
                var i = 0;
                var path = TempDirectoryPath + pendingMatchResult.Values.First().Value + ".tiff";
                while (File.Exists(path))
                {
                    ++i;
                    path = TempDirectoryPath + pendingMatchResult.Values.First().Value + i + ".tiff";
                }
                
                if (OcrEngine.Delete)
                    File.Move(file, path);
                else
                    File.Copy(file, path, true);
            }

            if (!OcrEngine.Debug && OcrEngine.Delete)
                file.DeleteIfExists();

            sb.Append(pendingMatchResult.Values.First().Value);
            return pendingMatchResult.Values.First().FullyTrustable;
        }

        public void ProcessLoadingHero(FilePath file, StringBuilder sb)
        {
            var tempPath = TempDirectoryPath + "temp.tiff";
            using (var image = ImageProcessingHelper.GetCroppeddHero(file))
                image.Save(tempPath);

            var pendingMatchResult = _engine.ProcessOcr(tempPath, OcrEngine.CandidateHeroes);
            if (!pendingMatchResult.Results.Any() || !pendingMatchResult.Values.First().Trustable)
            {
                return;
            }

            if (OcrEngine.Debug)
            {
                var i = 0;
                var path = TempDirectoryPath + pendingMatchResult.Values.First().Value + ".tiff";
                while (File.Exists(path))
                {
                    ++i;
                    path = TempDirectoryPath + pendingMatchResult.Values.First().Value + i + ".tiff";
                }

                if (!string.IsNullOrEmpty(sb.ToString()) || sb.ToString() != PickingText)
                {
                    if (OcrEngine.Delete)
                        File.Move(file, path);
                    else
                        File.Copy(file, path, true);
                }
            }
            else
                file.DeleteIfExists();

            sb.Append(pendingMatchResult.Values.First().Value);
        }

        private bool ProcessHero(FilePath file, float rotationAngle, StringBuilder sb, int offset, bool textInWhite)
        {
            var tempPath = TempDirectoryPath + "temp.tiff";
            var mode = textInWhite ? 0 : ImageProcessingHelper.CheckMode(file, rotationAngle);

            if (mode == -1)
            {
                if (!OcrEngine.Debug && OcrEngine.Delete)
                    file.DeleteIfExists();

                return false;
            }
            
            var startThresholding = LightModeThreshold;

            int sampleWidth;
            var image = ImageProcessingHelper.GetRotatedImage(rotationAngle, file, textInWhite, out sampleWidth);
            if (OcrEngine.Debug)
                image.Save(TempDirectoryPath + "RotatedImage.bmp");

            if (_engine is OcrEngineAsian)
                _engine.Engine.SetVariable(@"textord_min_xheight", 25);

            string pendingMatchResult = string.Empty;

            var scoreDictionary = new Dictionary<string, int>();

            // 130 - 135 - 125 - 140 - 120 - 145 - 115
            // 75 - 80 - 70 - 85 - 65 - 90 - 60
            var switcher = 0;
            int faultCount = 0;
            int failBinaryCheckCount = 0;
            for (var index = startThresholding + 15; index <= startThresholding + 30; index += switcher)
            {
                switcher = -switcher;
                if (switcher > 0)
                    switcher += offset;
                else
                    switcher -= offset;

                double count;
                var segmentationCount = ImageProcessingHelper.ProcessOnce(index, image, tempPath, rotationAngle, textInWhite, out count);
                
                if (segmentationCount == 0)
                {
                    failBinaryCheckCount ++;
                    if (failBinaryCheckCount > 5)
                    {
                        scoreDictionary.Clear();
                        break;
                    }

                    continue;
                }

                failBinaryCheckCount = -5;
                var newCount = count;
                if (segmentationCount < count && count - segmentationCount >= 2)
                    newCount = segmentationCount;
                
                OcrResult result;

                try
                {
                    result = _engine.ProcessOcr(newCount, tempPath,
                            OcrEngine.CandidateHeroes);
                }
                catch (Exception)
                {
                    return false;
                }

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
                    matchResultsWithMaxScore[0].Score *= 2;

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
                var path = TempDirectoryPath + (rotationAngle > 0 ? @"left\" : @"right\") + pendingMatchResult + ".tiff";
                while (File.Exists(path))
                {
                    ++i;
                    path = TempDirectoryPath + (rotationAngle > 0 ? @"left\" : @"right\") + pendingMatchResult + i + ".tiff";
                }

                if (!string.IsNullOrEmpty(sb.ToString()) || sb.ToString() != PickingText)
                {
                    if (OcrEngine.Delete)
                        File.Move(file, path);
                    else
                        File.Copy(file, path, true);
                }
            }


            sb.Append(pendingMatchResult);
            image.Dispose();


            if (!OcrEngine.Debug && OcrEngine.Delete)
                file.DeleteIfExists();

            return maxValue == int.MaxValue;
        }
    }
}