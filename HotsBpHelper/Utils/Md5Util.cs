using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HotsBpHelper.Utils
{
    public class Md5Util
    {
        public static string CaculateFileMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty);
                }
            }
        }

        public static string CaculateStringMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(filename))).Replace("-", String.Empty);
            }
        }
    }
}