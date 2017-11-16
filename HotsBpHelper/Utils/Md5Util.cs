using HashLib;
using System;
using System.IO;
using System.Text;

namespace HotsBpHelper.Utils
{
    public class Md5Util
    {
        public static string CaculateFileMd5(string filename)
        {
            var hash = HashFactory.Crypto.CreateMD5();
            using (var stream = File.OpenRead(filename))
            {
                var result = hash.ComputeStream(stream);
                return result.ToString().Replace("-", String.Empty);
            }
        }

        public static string CaculateStringMd5(string str)
        {
            var hash = HashFactory.Crypto.CreateMD5();
            var result = hash.ComputeBytes(Encoding.Default.GetBytes(str));
            return result.ToString().Replace("-", String.Empty);
        }
    }
}