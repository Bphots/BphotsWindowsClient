using System;
using System.IO;
using System.Text;
using HotsBpHelper.Utils;

namespace HotsBpHelper.Configuration
{
    public class BpServiceConfigParser : ConfigureFileParser
    {
        private static readonly string BpServiceConfigPath =
            Path.GetFullPath(@".\Service\config.ini");

        private const string TempFolderKey = "TempFolder";
        private const string DocumentFolderKey = "DocumentFolder";

        private const string LoadedKey = "BpServiceQuestionLoaded";

        public BpServiceConfigParser() : base(BpServiceConfigPath)
        {
        }

        public bool GetLoaded()
        {
            var isLoaded = GetConfigurationValue(LoadedKey);

            return !string.IsNullOrEmpty(isLoaded);
        }

        public string GetDocumentsFolder()
        {
            var documentsPath = GetConfigurationValue(Environment.UserName + DocumentFolderKey);

            return documentsPath;
        }

        public string GetTempFolder()
        {
            var documentsPath = GetConfigurationValue(Environment.UserName + TempFolderKey);

            return documentsPath;
        }

        public static void PopulateConfigurationSettings()
        {
            var parser = new BpServiceConfigParser();
            FileUtil.MyDocumentFolderPathFromConfig = parser.GetDocumentsFolder();
            FileUtil.MyTempFolderPathFromConfig = parser.GetTempFolder();
            if (!App.HasServiceAsked)
                App.HasServiceAsked = parser.GetLoaded();
        }

        public static void WriteConfig()
        {
            var parser = new BpServiceConfigParser();
            if (!string.IsNullOrEmpty(FileUtil.MyDocumentFolderPathFromConfig))
                parser.AllValues[Environment.UserName + DocumentFolderKey] = FileUtil.MyDocumentFolderPathFromConfig;
            if (!string.IsNullOrEmpty(FileUtil.MyTempFolderPathFromConfig))
                parser.AllValues[Environment.UserName + TempFolderKey] = FileUtil.MyTempFolderPathFromConfig;

            if (App.HasServiceAsked)
               parser.AllValues[LoadedKey] = "1";

            var sb = new StringBuilder();
            foreach (var tuple in parser.AllValues)
            {
                sb.AppendLine(WriteConfigurationValue(tuple.Key, tuple.Value));
            }

            File.WriteAllText(BpServiceConfigPath, sb.ToString());
        }
    }
}