using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotNetHelper;
using HotsBpHelper.Configuration;

namespace HotsBpHelper.Utils
{
    public static class FileUtil
    {
        public static void CleanUpImageTestFiles()
        {
            DirectoryInfo di = new DirectoryInfo(@".\Images\Heroes\Test");
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

        public static string GetMyDocumentFolderPath()
        {
            var myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (string.IsNullOrEmpty(myDocumentsPath))
                BpServiceConfigParser.PopulateConfigurationSettings();
            else
            {
                MyDocumentFolderPathFromConfig = myDocumentsPath;
                BpServiceConfigParser.WriteConfig();
            }

            return MyDocumentFolderPathFromConfig;
        }

        public static string MyDocumentFolderPathFromConfig = string.Empty;
    }
}