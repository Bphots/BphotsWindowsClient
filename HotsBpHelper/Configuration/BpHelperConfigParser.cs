using System;
using System.IO;
using System.Text;
using DotNetHelper;
using HotsBpHelper.Settings;

namespace HotsBpHelper.Configuration
{
    public class BpHelperConfigParser : ConfigureFileParser
    {
        private const string AutoShowHideHelperKey = @"AutoShowHideHelper";
        private const string AutoDetectHeroAndMapKey = @"AutoDetectHeroAndMap";
        private const string AutoShowMMRKey = @"AutoShowMMR";
        private const string AutoUploadNewReplayToHotslogsKey = @"AutoUploadReplayToHotslogs";
        private const string AutoUploadNewReplayToHotsweekKey = @"AutoUploadReplayToHotsweek";
        private const string UploadStrategyKey = @"UploadStrategy";
        private const string LanguageForBphotsKey = @"LanguageForBphots";
        private const string LanguageForRegionKey = @"LanguageForRegion";
        private const string DefalutReplayFolderPathKey = @"DefalutReplayFolderPath";
        private const string MMRAutoCloseTimeKey = @"MMRAutoCloseTime";

        private static readonly FilePath BpHelperConfigPath =
            Path.GetFullPath(@".\config.ini");

        private readonly string ProfilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Heroes of the Storm\Accounts");

        public BpHelperConfigParser() : base(BpHelperConfigPath)
        {
        }

        public bool GetAutoShowHideHelper()
        {
            var autoShowHideHelper = GetConfigurationValue(AutoShowHideHelperKey);

            return autoShowHideHelper != "0";
        }

        public bool GetAutoDetectHeroAndMap()
        {
            var autoDetectHeroAndMap = GetConfigurationValue(AutoDetectHeroAndMapKey);

            return autoDetectHeroAndMap != "0";
        }

        public bool GetAutoShowMMR()
        {
            var autoShowMmr = GetConfigurationValue(AutoShowMMRKey);

            return autoShowMmr != "0";
        }

        public bool GetAutoUploadNewReplayToHotslogs()
        {
            var autoUploadNewReplayToHotslogs = GetConfigurationValue(AutoUploadNewReplayToHotslogsKey);

            return autoUploadNewReplayToHotslogs != "0";
        }

        public bool GetAutoUploadNewReplayToHotsweek()
        {
            var autoUploadNewReplayToHotsweek = GetConfigurationValue(AutoUploadNewReplayToHotsweekKey);

            return autoUploadNewReplayToHotsweek != "0";
        }

        public UploadStrategy GeUploadStrategy()
        {
            var uploadStrategy = GetConfigurationValue(UploadStrategyKey);

            if (uploadStrategy == "2")
                return UploadStrategy.UploadAll;

            if (uploadStrategy == "1")
                return UploadStrategy.UploadNew;

            if (uploadStrategy == "0")
                return UploadStrategy.None;

            return UploadStrategy.UploadAll;
        }

        public string GetLanguageForBphots()
        {
            var languageForBpHots = GetConfigurationValue(LanguageForBphotsKey);

            return languageForBpHots;
        }

        public int GetMMRAutoCloseTime()
        {
            var mmrAutoCloseTime = GetConfigurationValue(MMRAutoCloseTimeKey);

            int time;
            if (int.TryParse(mmrAutoCloseTime, out time) && time >= 0 && time <= 60)
                return time;

            return 30;
        }

        public string GetDefalutReplayFolderPath()
        {
            var defalutReplayFolderPath = GetConfigurationValue(DefalutReplayFolderPathKey);

            if (Directory.Exists(defalutReplayFolderPath))
                return defalutReplayFolderPath;

            return ProfilePath;
        }

        public static void PopulateConfigurationSettings(CustomConfigurationSettings customConfigurationSettings)
        {
            var parser = new BpHelperConfigParser();
            customConfigurationSettings.AutoDetectHeroAndMap = parser.GetAutoDetectHeroAndMap();
            customConfigurationSettings.AutoShowHideHelper = parser.GetAutoShowHideHelper();
            customConfigurationSettings.AutoShowMMR = parser.GetAutoShowMMR();
            customConfigurationSettings.AutoUploadReplayToHotslogs = parser.GetAutoUploadNewReplayToHotslogs();
            customConfigurationSettings.AutoUploadReplayToHotsweek = parser.GetAutoUploadNewReplayToHotsweek();
            customConfigurationSettings.DefalutReplayFolderPath = parser.GetDefalutReplayFolderPath();
            customConfigurationSettings.UploadStrategy = parser.GeUploadStrategy();
            customConfigurationSettings.MMRAutoCloseTime = parser.GetMMRAutoCloseTime();
        }

        public static void WriteConfig(CustomConfigurationSettings customConfigurationSettings)
        {
            var sb = new StringBuilder();
            {
                sb.AppendLine(WriteConfigurationValue(AutoDetectHeroAndMapKey, customConfigurationSettings.AutoDetectHeroAndMap));
                sb.AppendLine(WriteConfigurationValue(AutoShowHideHelperKey, customConfigurationSettings.AutoShowHideHelper));
                sb.AppendLine(WriteConfigurationValue(AutoShowMMRKey, customConfigurationSettings.AutoShowMMR));
                sb.AppendLine(WriteConfigurationValue(AutoUploadNewReplayToHotslogsKey, customConfigurationSettings.AutoUploadReplayToHotslogs));
                sb.AppendLine(WriteConfigurationValue(AutoUploadNewReplayToHotsweekKey, customConfigurationSettings.AutoUploadReplayToHotsweek));
                sb.AppendLine(WriteConfigurationValue(DefalutReplayFolderPathKey, customConfigurationSettings.DefalutReplayFolderPath));
                sb.AppendLine(WriteConfigurationValue(UploadStrategyKey, (int)customConfigurationSettings.UploadStrategy));
                sb.AppendLine(WriteConfigurationValue(MMRAutoCloseTimeKey, customConfigurationSettings.MMRAutoCloseTime));
            }

            File.WriteAllText(BpHelperConfigPath, sb.ToString());
        }
    }
}