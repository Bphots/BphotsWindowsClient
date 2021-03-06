﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using HotsBpHelper.Settings;
using ImageProcessor.Ocr;
using WPFLocalizeExtension.Engine;

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
        private const string LanguageForMessageKey = @"LanguageForMessage";
        private const string LanguageForGameClientKey = @"LanguageForGameClient";
        private const string MMRAutoCloseTimeKey = @"MMRAutoCloseTime";
        private const string UploadBanSampleKey = @"UploadBanSample";
        private const string PlayerTagKey = @"PlayerTag";
        private const string HotsweekPlayerIdKey = @"HotsweekPlayerId";
        private const string HotKeySettingsKey = @"HotKey";

        private static readonly string BpHelperConfigPath =
            Path.GetFullPath(@".\config.ini");

        private static readonly Dictionary<string, string> _hotKeys = new Dictionary<string, string>() {
            { "ShowHideHelper", "B" },
            { "ShowMmr",        "M" },
            { "AutoDetect",     "R" },
            { "ResetHelper",    "N" },
            { "ShowDevTool",    "D" },
            { "CaptureScreen",  "C" }
        };

        public BpHelperConfigParser() : base(BpHelperConfigPath)
        {
        }

        public bool GetAutoShowHideHelper()
        {
            var autoShowHideHelper = GetConfigurationValue(AutoShowHideHelperKey);

            return autoShowHideHelper != "0";
        }

        public List<string> GetPlayerTags()
        {
            var playerTagString = GetConfigurationValue(PlayerTagKey);
            if (string.IsNullOrEmpty(playerTagString))
                return new List<string>();

            var playerTags = playerTagString.Split('|');
            return playerTags.ToList();
        }

        public string GetHotsweekPlayerId()
        {
            var hotsweekPlayerIdString = GetConfigurationValue(HotsweekPlayerIdKey);
            return hotsweekPlayerIdString;
        }

        public bool GetUploadBanSample()
        {
            var uploadBanSample = GetConfigurationValue(UploadBanSampleKey);

            return uploadBanSample != "0";
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

        public string GetHotKeySettings(string key)
        {
            var hotkey = GetConfigurationValue(HotKeySettingsKey + key);

            return hotkey == string.Empty ? _hotKeys[key] : hotkey;
        }

        public string GetLanguageForBphots()
        {
            var languageForBpHots = GetConfigurationValue(LanguageForBphotsKey);
            if (!string.IsNullOrEmpty(languageForBpHots))
                return languageForBpHots;

            var languageFromGame = GetLanguageFromGame();
            if (!string.IsNullOrEmpty(languageFromGame))
                return languageFromGame;

            return GetLanguageFromSystem();
        }

        public string GetLanguageForMessage()
        {
            var languageForMessage = GetConfigurationValue(LanguageForMessageKey);
            if (!string.IsNullOrEmpty(languageForMessage))
                return languageForMessage;

            var languageFromGame = GetLanguageFromGame();
            if (!string.IsNullOrEmpty(languageFromGame))
                return languageFromGame;

            return GetLanguageFromSystem();
        }

        public string GetLanguageForGameClient()
        {
            var languageForGameClient = GetConfigurationValue(LanguageForGameClientKey);
            if (!string.IsNullOrEmpty(languageForGameClient))
                return languageForGameClient;

            var languageFromGame = GetLanguageFromGame();
            if (!string.IsNullOrEmpty(languageFromGame))
                return languageFromGame;

            return string.Empty;
        }

        private static string GetLanguageFromSystem()
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.InstalledUICulture;
            switch (LocalizeDictionary.Instance.Culture.Name)
            {
                case "zh-CN":
                case "zh-CHS":
                    return "zh-CN";
                case "ko-KR":
                    return "ko-KR";
                case "zh-TW":
                case "zh-HK":
                case "zh-CHT":
                    return "zh-TW";
                default:
                    return "en-US";
            }
        }

        private static string GetLanguageFromGame()
        {
            var config = new HotsVariableConfigParser();
            var locale = config.CheckTextLocale();
            if (!string.IsNullOrEmpty(locale))
            {
                switch (locale)
                {
                    case "zhCN":
                        return "zh-CN";
                    case "koKR":
                        return "ko-KR";
                    case "zhTW":
                        return "zh-TW";
                    case "enUS":
                        return "en-US";
                    default:
                        return string.Empty;
                }
            }
            return string.Empty;
        }

        public int GetMMRAutoCloseTime()
        {
            var mmrAutoCloseTime = GetConfigurationValue(MMRAutoCloseTimeKey);

            int time;
            if (int.TryParse(mmrAutoCloseTime, out time) && time >= 0 && time <= 60)
                return time;

            return 30;
        }

        public static void PopulateConfigurationSettings(CustomConfigurationSettings customConfigurationSettings)
        {
            var parser = new BpHelperConfigParser();
            customConfigurationSettings.AutoDetectHeroAndMap = parser.GetAutoDetectHeroAndMap();
            customConfigurationSettings.AutoShowHideHelper = parser.GetAutoShowHideHelper();
            customConfigurationSettings.UploadBanSample = parser.GetUploadBanSample();
            customConfigurationSettings.AutoShowMMR = parser.GetAutoShowMMR();
            customConfigurationSettings.AutoUploadReplayToHotslogs = parser.GetAutoUploadNewReplayToHotslogs();
            customConfigurationSettings.AutoUploadReplayToHotsweek = parser.GetAutoUploadNewReplayToHotsweek();
            customConfigurationSettings.UploadStrategy = parser.GeUploadStrategy();
            customConfigurationSettings.MMRAutoCloseTime = parser.GetMMRAutoCloseTime();

            foreach (var hotKey in _hotKeys)
            {
                customConfigurationSettings.HotKeySettings[hotKey.Key] = parser.GetHotKeySettings(hotKey.Key);
            }

            App.UserDataSettings.PlayerTags = parser.GetPlayerTags();
            App.UserDataSettings.HotsweekPlayerId = parser.GetHotsweekPlayerId();

            customConfigurationSettings.LanguageForBphots = parser.GetLanguageForBphots();
            App.Language = customConfigurationSettings.LanguageForBphots;
            LocalizeDictionary.Instance.Culture =
                CultureInfo.GetCultureInfo(customConfigurationSettings.LanguageForBphots);

            customConfigurationSettings.LanguageForMessage = parser.GetLanguageForMessage();

            customConfigurationSettings.LanguageForGameClient = parser.GetLanguageForGameClient();
            SetOcrLanguage(customConfigurationSettings.LanguageForGameClient);
        }

        private static void SetOcrLanguage(string languageForGameClient)
        {
            if (string.IsNullOrEmpty(languageForGameClient))
            {
                App.OcrLanguage = OcrLanguage.Unavailable;
                return;
            }

            switch (languageForGameClient)
            {
                case "zh-CN":
                    App.OcrLanguage = OcrLanguage.SimplifiedChinese;
                    break;
                case "ko-KR":
                    App.OcrLanguage = OcrLanguage.Unavailable;
                    break;
                case "zh-TW":
                    App.OcrLanguage = OcrLanguage.TraditionalChinese;
                    break;
                case "en-US":
                    App.OcrLanguage = OcrLanguage.English;
                    break;
                default:
                    App.OcrLanguage = OcrLanguage.Unavailable;
                    break;
            }
        }

        public static void WriteConfig(CustomConfigurationSettings customConfigurationSettings)
        {
            var sb = new StringBuilder();
            {
                sb.AppendLine(WriteConfigurationValue(AutoDetectHeroAndMapKey,
                    customConfigurationSettings.AutoDetectHeroAndMap));
                sb.AppendLine(WriteConfigurationValue(AutoShowHideHelperKey,
                    customConfigurationSettings.AutoShowHideHelper));
                sb.AppendLine(WriteConfigurationValue(AutoShowMMRKey, customConfigurationSettings.AutoShowMMR));
                sb.AppendLine(WriteConfigurationValue(AutoUploadNewReplayToHotslogsKey,
                    customConfigurationSettings.AutoUploadReplayToHotslogs));
                sb.AppendLine(WriteConfigurationValue(UploadBanSampleKey, customConfigurationSettings.UploadBanSample));
                sb.AppendLine(WriteConfigurationValue(AutoUploadNewReplayToHotsweekKey,
                    customConfigurationSettings.AutoUploadReplayToHotsweek));
                sb.AppendLine(WriteConfigurationValue(UploadStrategyKey,
                    (int) customConfigurationSettings.UploadStrategy));
                sb.AppendLine(WriteConfigurationValue(MMRAutoCloseTimeKey, customConfigurationSettings.MMRAutoCloseTime));
                sb.AppendLine(WriteConfigurationValue(LanguageForBphotsKey,
                    customConfigurationSettings.LanguageForBphots));
                sb.AppendLine(WriteConfigurationValue(LanguageForMessageKey,
                    customConfigurationSettings.LanguageForMessage));

                foreach (var key in _hotKeys)
                {
                    sb.AppendLine(WriteConfigurationValue(HotKeySettingsKey + key.Key,
                        customConfigurationSettings.HotKeySettings[key.Key]));
                }

                var languageFromGame = GetLanguageFromGame();
                if (string.IsNullOrEmpty(languageFromGame))
                    sb.AppendLine(WriteConfigurationValue(LanguageForGameClientKey,
                        customConfigurationSettings.LanguageForGameClient));
            }

            File.WriteAllText(BpHelperConfigPath, sb.ToString());
        }
    }
}