using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace HotsBpHelper.Utils
{
    public class LogUtil
    {
        public static bool NoLog = false;
        private readonly string _relativePath;
        private readonly StringBuilder _sb = new StringBuilder();

        public LogUtil(string relativePath)
        {
            _relativePath = relativePath;
        }

        public void Log(string text)
        {
            if (NoLog)
                return;

            _sb.AppendLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                CultureInfo.InvariantCulture) + @" : " + text);
        }

        public void Flush()
        {
            if (NoLog)
                return;

            Log("Complete");
            File.WriteAllText(_relativePath, _sb.ToString());
        }
    }
}