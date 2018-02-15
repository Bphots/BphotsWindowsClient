using System;
using System.IO;

namespace HotsBpHelper.Configuration
{
    public class HotsVariableConfigParser : ConfigureFileParser
    {
        private const string WindowlessKey = @"displaymode";
        private const string Locale = @"localeiddata";
        private const string WindowlessValue = @"1";

        private static readonly string HotsVariablePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Heroes of the Storm\Variables.txt");

        public HotsVariableConfigParser() : base(HotsVariablePath)
        {
        }

        public bool CheckIfWindowlessMax()
        {
            var windowstate = GetConfigurationValue(WindowlessKey);

            return string.IsNullOrEmpty(windowstate) || windowstate == WindowlessValue;
        }

        public string CheckTextLocale()
        {
            var locale = GetConfigurationValue(Locale);

            return locale;
        }
    }
}