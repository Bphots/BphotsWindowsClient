using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetHelper;

namespace HotsBpHelper.Configuration
{
    public class ConfigureFileParser
    {
        private readonly FilePath _configrationFilePath;

        public ConfigureFileParser(FilePath configurationFilePath)
        {
            _configrationFilePath = configurationFilePath;
            InitializeConfiguration();
        }

        public void InitializeConfiguration()
        {
            if (!_configrationFilePath.Exists())
                return;

            var lines = File.ReadAllLines(_configrationFilePath);
            foreach (var line in lines.Select(l => l.Trim()))
            {
                var valuePair = line.Split('=').Select(v => v.Trim()).ToList();
                if (valuePair.Count() < 2)
                    continue;

                _configurationDictionary[valuePair[0]] = valuePair[1];
            }
        }

        protected string GetConfigurationValue(string key)
        {
            if (_configurationDictionary.ContainsKey(key))
                return _configurationDictionary[key].Trim();

            return string.Empty;
        }

        protected static string WriteConfigurationValue(string key, object value)
        {
            if (value is bool)
                return key + "=" + ((bool)value ? 1 : 0);

            return key + "=" + value;
        }

        private readonly Dictionary<string, string> _configurationDictionary = new Dictionary<string, string>();
    }
}
