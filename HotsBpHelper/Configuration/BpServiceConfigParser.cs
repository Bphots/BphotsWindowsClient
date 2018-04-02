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

        public BpServiceConfigParser() : base(BpServiceConfigPath)
        {
        }

        public string GetDocumentsFolder()
        {
            var documentsPath = GetConfigurationValue(Environment.UserName);

            return documentsPath;
        }

        public static void PopulateConfigurationSettings()
        {
            var parser = new BpServiceConfigParser();
            FileUtil.MyDocumentFolderPathFromConfig = parser.GetDocumentsFolder();
        }

        public static void WriteConfig()
        {
            var parser = new BpServiceConfigParser();
            parser.AllValues[Environment.UserName] = FileUtil.MyDocumentFolderPathFromConfig;

            var sb = new StringBuilder();
            foreach (var tuple in parser.AllValues)
            {
                sb.AppendLine(WriteConfigurationValue(tuple.Key, tuple.Value));
            }

            File.WriteAllText(BpServiceConfigPath, sb.ToString());
        }
    }
}