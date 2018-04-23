using System;
using System.IO;
using System.Linq;
using HotsBpHelper.Configuration;

namespace HotsBpHelper.Utils
{
    public static class FileUtil
    {
        public static void CleanUpImageTestFiles()
        {
            var di = new DirectoryInfo(@".\Images\Heroes\Test");
            if (!di.Exists)
                return;

            try
            {
                foreach (var file in di.GetFiles().ToList())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}