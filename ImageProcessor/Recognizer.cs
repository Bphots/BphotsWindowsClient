using System;
using System.IO;
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

        private readonly OcrEngine m_engine;

        public static DirPath TempDirectoryPath;

        private bool m_isDarkMode;

        public string PickingText => m_engine.PickingText;


        public Recognizer(OcrLanguage language, string sourceDirectory = null)
        {
            m_engine = OcrEngine.CreateEngine(language);
            
            TempDirectoryPath = sourceDirectory + @"\Test\";
            if (!Directory.Exists(TempDirectoryPath))
                Directory.CreateDirectory(TempDirectoryPath);
        }

        public void Dispose()
        {
            m_engine.Dispose();
        }

        /// <summary>
        ///     Processing the file with a rotating angle
        /// </summary>
        /// <param name="file"></param>
        /// <param name="angle"></param>
        /// <param name="sb"></param>
        public bool Recognize(string file, float angle, StringBuilder sb, string tempId)
        {
            return ProcessHero(file, angle, sb, tempId);
        }

        public void ProcessMap(FilePath file, StringBuilder sb)
        {
            var tempPath = TempDirectoryPath + "temp.tiff";
            using (var image = ImageProcessingHelper.GetCroppedMap(file))
                image.Save(tempPath);

            var pendingMatchResult = m_engine.ProcessOcr(tempPath, OcrEngine.CandidateMaps);
            if (!pendingMatchResult.Trustable)
            {
                ResetFlags();
                return;
            }

            var i = 0;
            var path = TempDirectoryPath + pendingMatchResult.Value + ".tiff";
            while (File.Exists(path))
            {
                ++i;
                path = TempDirectoryPath + pendingMatchResult.Value + i + ".tiff";
            }

            sb.Append(pendingMatchResult.Value);

            File.Move(file, path);
            ResetFlags();
        }

        private bool ProcessHero(FilePath file, float rotationAngle, StringBuilder sb, string tempId)
        {
            var tempPath = TempDirectoryPath + "temp" + tempId + ".tiff";
            int startThresholding;
            double count;
            var mode = ImageProcessingHelper.CheckMode(file, rotationAngle);
            if (mode == -1)
                return false;

            m_isDarkMode = mode == 0;
            startThresholding = m_isDarkMode ? DarkModeThreshold : LightModeThreshold;

            int sampleWidth;
            var image = ImageProcessingHelper.GetCroppedImage(rotationAngle, file, m_isDarkMode, out sampleWidth);
            if (OcrEngine.Debug)
                image.Save(TempDirectoryPath + "CroppedImage.bmp");
            //image.Save(@"D:\qqytqqyt\Documents\HeroesBpProject\OcrHelper\HotsBpHelper\bin\Debug\Images\Heroes\5.bmp");
            //image.SaveImage(m_tempDirectoryPath + file.GetFileNameWithoutExtension() + "rotated.tiff");
            var segmentationCount = ImageProcessingHelper.ProcessOnce(startThresholding + 15, image, tempPath);
            count = (double) image.Width/sampleWidth*7.5 + 0.1;


            var pendingMatchResult = m_engine.ProcessOcr(Math.Min(count, segmentationCount), tempPath,
                 OcrEngine.CandidateHeroes);
            if (!pendingMatchResult.FullyTruestable)
            {
                // 130 - 135 - 125 - 140 - 120 - 145 - 115
                // 75 - 80 - 70 - 85 - 65 - 90 - 60
                var switcher = 0;
                for (var index = startThresholding + 15; index <= startThresholding + 30; index += switcher)
                {
                    var segmentationCountNew = ImageProcessingHelper.ProcessOnce(index, image, tempPath);
                    var result = m_engine.ProcessOcr(Math.Min(count, segmentationCountNew), tempPath,
                        OcrEngine.CandidateHeroes);

                    if (OcrEngine.Debug)
                        Console.WriteLine(@"Thresdhold " + index + @" : " + result.Key.Replace("\n", string.Empty) + @" => " + result.Value);
                    // File.Copy(m_tempPath, m_tempPath.GetDirPath() + index + " " + result.Key.Replace("\n", string.Empty) + "_" + result.Value + ".tiff", true);
                    if (!result.InDoubt && string.IsNullOrEmpty(result.Value))
                    {
                        pendingMatchResult = result;
                        break;
                    }
                    if (result.Score > pendingMatchResult.Score)
                        pendingMatchResult = result;

                    if (result.Trustable)
                        break;

                    if (switcher > 0)
                        switcher += 5;
                    else
                        switcher -= 5;

                    switcher = -switcher;
                }
            }

            var i = 0;
            var path = TempDirectoryPath + pendingMatchResult.Key.Replace("\n", string.Empty) + " - " + pendingMatchResult.Value + ".tiff";
            while (File.Exists(path))
            {
                ++i;
                path = TempDirectoryPath + pendingMatchResult.Key.Replace("\n", string.Empty) + " - " + pendingMatchResult.Value + i + ".tiff";
            }

            sb.Append(pendingMatchResult.Value);
            image.Dispose();
            File.Copy(file, path, true);
            ResetFlags();

            return pendingMatchResult.FullyTruestable;
        }

        private void ResetFlags()
        {
            m_isDarkMode = false;
        }
    }
}