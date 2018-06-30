﻿using System;
using System.IO;
using System.Text;
using HotsBpHelper.Utils;

namespace HotsBpHelper.Configuration
{
    public class BpServiceConfigParser : ConfigureFileParser
    {
        private static readonly string BpServiceConfigPath =
            Path.GetFullPath(@".\Service\Variables.ini");
        
        private const string LoadedKey = "BpServiceQuestionLoaded";
        private const string HotsWeekAskedKey = "BpWeekQuestionAsked";

        public BpServiceConfigParser() : base(BpServiceConfigPath)
        {
        }

        public bool GetLoaded()
        {
            var isLoaded = GetConfigurationValue(LoadedKey);

            return !string.IsNullOrEmpty(isLoaded);
        }

        public bool GetHotsWeekAsked()
        {
            var asked = GetConfigurationValue(HotsWeekAskedKey);

            return !string.IsNullOrEmpty(asked);
        }

        public static void PopulateConfigurationSettings()
        {
            var parser = new BpServiceConfigParser();
            if (!App.HasServiceAsked)
                App.HasServiceAsked = parser.GetLoaded();
            if (!App.HasHotsWeekAsked)
                App.HasHotsWeekAsked = parser.GetHotsWeekAsked();
        }

        public static void WriteConfig()
        {
            var parser = new BpServiceConfigParser();

            if (App.HasServiceAsked)
               parser.AllValues[LoadedKey] = "1";
            if (App.HasHotsWeekAsked)
               parser.AllValues[HotsWeekAskedKey] = "1";

            var sb = new StringBuilder();
            foreach (var tuple in parser.AllValues)
            {
                sb.AppendLine(WriteConfigurationValue(tuple.Key, tuple.Value));
            }

            File.WriteAllText(BpServiceConfigPath, sb.ToString());
        }
    }
}