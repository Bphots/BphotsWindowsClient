using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HotsBpHelper.Utils
{
    public class FileUtil
    {
        public static string CheckMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty);
                }
            }
        }
    }
}