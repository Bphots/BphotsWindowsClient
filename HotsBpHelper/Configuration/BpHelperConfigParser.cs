using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotsBpHelper.Configuration
{
    public class BpHelperConfigParser : ConfigureFileParser
    {
        private const string CustomZoomPercentageKey = @"CustomZoomPercentage";

        public BpHelperConfigParser() : base(BpHelperConfigPath)
        {
        }

        public double GetBrowserZoomValue()
        {
            var customZoomPercentage = GetConfigurationValue(CustomZoomPercentageKey);

            double zoom;
            if (string.IsNullOrEmpty(customZoomPercentage) || !double.TryParse(customZoomPercentage, out zoom))
                return -100;

            double cefZoomValue = 4 - 400 / zoom;

            return cefZoomValue;
        }

        private static readonly string BpHelperConfigPath =
            Path.GetFullPath(@".\config.ini");
    }
}
