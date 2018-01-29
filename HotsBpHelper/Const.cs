using System.IO;

namespace HotsBpHelper
{
    public static class Const
    {
        public const string WEB_API_ROOT = "https://www.bphots.com/bp_helper/";

        public const string LOCAL_WEB_FILE_DIR = "WebFiles";
        
        public const string PATCH = "18013002";

        public const string UPDATE_FEED_XML = "https://www.bphots.com/bp_helper/get/update?patch=" + PATCH;

        public const string HEROES_PROCESS_NAME = "HeroesOfTheStorm";

        public const string HOTSBPHELPER_PROCESS_NAME = "HotsBpHelper";

        public const string ABOUT_URL = "https://www.bphots.com/articles/base/about";

        public const string HELP_URL = "https://www.bphots.com/articles/base/help";

        public static readonly string BattleLobbyPath = Path.Combine(Path.GetTempPath(), @"Heroes of the Storm\TempWriteReplayP1\replay.server.battlelobby");
    }
}