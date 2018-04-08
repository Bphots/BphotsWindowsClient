using System;
using System.IO;
using System.Linq;
using HotsBpHelper.Configuration;

namespace HotsBpHelper.Utils
{
    public static class FileUtil
    {
        public static string MyDocumentFolderPathFromConfig = string.Empty;
        public static string MyTempFolderPathFromConfig = string.Empty;

        private static bool IsLaunchedFromService
            => string.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        public static void CleanUpImageTestFiles()
        {
            var di = new DirectoryInfo(@".\Images\Heroes\Test");
            if (!di.Exists)
                return;

            foreach (var file in di.GetFiles().ToList())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}