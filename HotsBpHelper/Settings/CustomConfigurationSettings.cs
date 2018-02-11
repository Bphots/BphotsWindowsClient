namespace HotsBpHelper.Settings
{
    public class CustomConfigurationSettings
    {
        public bool AutoShowHideHelper { get; set; }

        public bool AutoDetectHeroAndMap { get; set; }

        public bool AutoShowMMR { get; set; } = true;

        public bool AutoUploadNewReplayToHotslogs { get; set; } 

        public bool AutoUploadNewReplayToHotsweek { get; set; } 

        public UploadStrategy UploadStrategy { get; set; }

        public string LanguageForBphots { get; set; }

        public string LanguageForRegion { get; set; }

        public int MMRAutoCloseTime { get; set; }

        public string DefalutReplayFolderPath { get; set; }
    }

    public enum UploadStrategy
    {
        UploadAll = 2,
        UploadNew = 1,
        None = 0
    }
}
