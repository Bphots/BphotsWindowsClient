using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetHelper;
using HotsBpHelper.ConfigurationHelper;

namespace HotsBpHelper.Configuration
{
    public class HotsVariableConfigParser : ConfigureFileParser
    {
        public HotsVariableConfigParser() : base(HotsVariablePath)
        {}

        public bool CheckIfWindowlessMax()
        {
            var windowstate = GetConfigurationValue(WindowlessKey);

            return string.IsNullOrEmpty(windowstate) || windowstate == WindowlessValue;
        }

        private static readonly string HotsVariablePath =
            Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), @"Documents\Heroes of the Storm\Variables.txt");

        private const string WindowlessKey = @"displaymode";
        private const string WindowlessValue = @"1";
    }
}
