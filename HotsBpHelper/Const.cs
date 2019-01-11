using System;
using System.Globalization;
using System.IO;
using HotsBpHelper.Utils;

namespace HotsBpHelper
{
    public static class Const
    {
        public const string WEB_API_ROOT = "https://www.bphots.com/bp_helper/";

        public const string WEB_API_WEEK_ROOT = "https://www.bphots.com/week/battlereport/";

        public const string LOCAL_WEB_FILE_DIR = "WebFiles";

        public const string OSS_ADDRESS = "http://bphots-1251808214.cossh.myqcloud.com/bp_helper/client/inform.txt";

        public const string PATCH = "19011101";

        public const string UPDATE_FEED_XML = "https://www.bphots.com/bp_helper/get/update?patch=" + PATCH;

        public const string HEROES_PROCESS_NAME = "HeroesOfTheStorm";

        public const string HOTSBPHELPER_PROCESS_NAME = "HotsBpHelper";

        public const string ABOUT_URL = "https://www.bphots.com/articles/base/about";

        public const string HELP_URL = "https://www.bphots.com/articles/base/help";

        public static readonly string BattleLobbyPath = Path.Combine(Path.GetTempPath(), @"Heroes of the Storm\TempWriteReplayP1\replay.server.battlelobby");
        
        public static readonly string ProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Heroes of the Storm\Accounts");

        public const int BestExpericenResolutionHeight = 760;

        public const int IncompatibleResolutionHeight = 760;

        public const int ReplayMinimumBuild = 43905;

        public const string ServiceName = "HotsBpHelper - Monitor";

        public static readonly DateTime HotsweekAcceptTime = DateTime.Parse("2018-07-12T00:00:00Z", new CultureInfo("en-US")).ToUniversalTime();

        public static readonly DateTime HotsweekReportTime = DateTime.Parse("2018-07-22T09:00:00Z", new CultureInfo("en-US")).ToUniversalTime();
    }
}