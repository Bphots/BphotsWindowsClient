using System.Collections.Generic;

namespace HotsBpHelper.Settings
{
    public class CustomConfigurationSettings
    {
        public string HotsWeekPlayerId { get; set; }

        public bool AutoShowHideHelper { get; set; }

        public bool AutoDetectHeroAndMap { get; set; }

        public bool AutoShowMMR { get; set; } = true;

        public bool AutoUploadReplayToHotslogs { get; set; } 

        public bool AutoUploadReplayToHotsweek { get; set; } 

        public UploadStrategy UploadStrategy { get; set; }

        public string LanguageForBphots { get; set; }

        public string LanguageForMessage { get; set; }

        public string LanguageForGameClient { get; set; }

        public int MMRAutoCloseTime { get; set; }

        public bool UploadBanSample { get; set; }
        
        public List<string> PlayerTags { get; set; }
    }

    public enum UploadStrategy
    {
        UploadAll = 2,
        UploadNew = 1,
        None = 0
    }
}
