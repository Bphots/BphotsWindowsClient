using Stylet;

namespace HotsBpHelper.Models
{
    public class FileUpdateInfo : PropertyChangedBase
    {
        public string FileName { get; set; }
        public string Url { get; set; }

        public string FileStatus { get; set; }

        public string RemoteMD5 { get; set; }

        public string LocalFilePath { get; set; }
    }
}