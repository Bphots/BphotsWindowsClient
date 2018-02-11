using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace HotsBpHelper.Uploader
{
    public class ReplayIdentity
    {
        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("fingerprint")]
        public string FingerPrint { get; set; }
    }


    [Serializable]
    public class ReplayFile : INotifyPropertyChanged
    {
        private UploadStatus _hotsWeekUploadStatus = UploadStatus.None;

        private bool _deleted;

        private UploadStatus _hotsApiUploadStatus = UploadStatus.None;

        public ReplayFile()
        {
            // Required for serialization
        } 

        public ReplayFile(string filename)
        {
            Filename = filename;
            Created = File.GetCreationTime(filename);
        }

        public bool NeedUpdate()
        {
            if (App.CustomConfigurationSettings.AutoUploadReplayToHotslogs)
                return _hotsApiUploadStatus == UploadStatus.None;
            
            if (App.CustomConfigurationSettings.AutoUploadReplayToHotsweek)
                return _hotsWeekUploadStatus == UploadStatus.None || _hotsWeekUploadStatus == UploadStatus.Reserved; ;

            return false;
        }

        public bool Settled()
        {
            var ignored = new[] { UploadStatus.None, UploadStatus.UploadError, UploadStatus.InProgress };
            return !ignored.Contains(_hotsApiUploadStatus) && !ignored.Contains(_hotsWeekUploadStatus);
        }

        [XmlIgnore]
        [JsonIgnore]
        public string Fingerprint { get; set; }

        public string Filename { get; set; }

        public DateTime Created { get; set; }

        public bool Deleted
        {
            get { return _deleted; }
            set
            {
                if (_deleted == value)
                {
                    return;
                }

                _deleted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Deleted)));
            }
        }

        public UploadStatus HotsWeekUploadStatus
        {
            get { return _hotsWeekUploadStatus; }
            set
            {
                if (_hotsWeekUploadStatus == value)
                {
                    return;
                }

                _hotsWeekUploadStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UploadStatus)));
            }
        }

        public UploadStatus HotsApiUploadStatus
        {
            get { return _hotsApiUploadStatus; }
            set
            {
                if (_hotsApiUploadStatus == value)
                {
                    return;
                }

                _hotsApiUploadStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UploadStatus)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return Filename;
        }

        public class ReplayFileComparer : IEqualityComparer<ReplayFile>
        {
            public bool Equals(ReplayFile x, ReplayFile y)
            {
                return x.Filename == y.Filename && x.Created == y.Created;
            }

            public int GetHashCode(ReplayFile obj)
            {
                return obj.Filename.GetHashCode() ^ obj.Created.GetHashCode();
            }
        }
    }
}