﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using HotsBpHelper.Utils;

namespace HotsBpHelper.Configuration
{
    public class UserDataConfigParser : ConfigureFileParser
    {
        private static readonly string UserDataConfigPath =
            Path.GetFullPath(@".\UserData.ini");

        private const string FMT = "O";

        private const string PlayerTagKey = @"PlayerTag";
        private const string HotsweekPlayerIdKey = @"HotsweekPlayerId";
        private const string LastClientVisitKey = @"LastClientVisit";

        public UserDataConfigParser() : base(UserDataConfigPath)
        {
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

        public DateTime GetLastClientVisit()
        {
            var lastClientVisit = GetConfigurationValue(LastClientVisitKey);
            if (string.IsNullOrEmpty(lastClientVisit))
                return DateTime.Now;

            DateTime dateTime;
            if (DateTime.TryParseExact(lastClientVisit, FMT, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                return dateTime;
            
            return DateTime.Now;
        }

        public static void PopulateUserDataSettings()
        {
            var parser = new UserDataConfigParser();

            var playerTags = parser.GetPlayerTags();
            if (playerTags.Any())
                App.UserDataSettings.PlayerTags = playerTags;

            var hotsweekPlayerId = parser.GetHotsweekPlayerId();
            if (!string.IsNullOrEmpty(hotsweekPlayerId))
                App.UserDataSettings.HotsweekPlayerId = hotsweekPlayerId;

            App.UserDataSettings.LastClientVisit = parser.GetLastClientVisit();
        }

        public static void WriteConfig()
        {
            var sb = new StringBuilder();

            sb.AppendLine(WriteConfigurationValue(PlayerTagKey,
                string.Join("|", App.UserDataSettings.PlayerTags)));
            sb.AppendLine(WriteConfigurationValue(HotsweekPlayerIdKey,
                 App.UserDataSettings.HotsweekPlayerId));
            sb.AppendLine(WriteConfigurationValue(LastClientVisitKey,
                 DateTime.Now.ToString(FMT)));

            File.WriteAllText(UserDataConfigPath, sb.ToString());
        }
    }
}