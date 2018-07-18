using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Heroes.ReplayParser;
using HotsBpHelper.Configuration;
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
        private bool _deleted;

        private UploadStatus _hotsApiUploadStatus = UploadStatus.None;
        private UploadStatus _hotsweekUploadStatus = UploadStatus.None;

        public ReplayFile()
        {
            // Required for serialization
        }

        public ReplayFile(string filename)
        {
            Filename = filename;
            Created = File.GetCreationTime(filename);
        }

        [XmlIgnore]
        [JsonIgnore]
        public string Fingerprint { get; set; }

        public string Filename { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public GameMode GameMode { get; set; }

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
        
        [XmlIgnore]
        public string HotsweekUploadStatusText => HotsweekUploadStatus.ToString();

        [XmlIgnore]
        public string HotsApiUploadStatusText => HotsApiUploadStatus.ToString();

        [XmlElement("HotsWeekUploadStatus")]
        [JsonIgnore]
        public UploadStatus HotsweekUploadStatus
        {
            get { return _hotsweekUploadStatus; }
            set
            {
                if (_hotsweekUploadStatus == value)
                {
                    return;
                }

                _hotsweekUploadStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UploadStatus)));
            }
        }

        [JsonIgnore]
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

        public bool NeedHotsApiUpdate()
        {
            bool needUpdate = false;
            if (App.CustomConfigurationSettings.AutoUploadReplayToHotslogs)
                needUpdate = _hotsApiUploadStatus == UploadStatus.None ||
                             _hotsApiUploadStatus == UploadStatus.UploadError ||
                             _hotsApiUploadStatus == UploadStatus.InProgress;

            return needUpdate;
        }

        public bool NeedHotsweekUpdate()
        {
            bool needUpdate = false;
            if (App.CustomConfigurationSettings.AutoUploadReplayToHotsweek)
                needUpdate = _hotsweekUploadStatus == UploadStatus.None ||
                             _hotsweekUploadStatus == UploadStatus.Reserved ||
                             _hotsweekUploadStatus == UploadStatus.UploadError ||
                             _hotsweekUploadStatus == UploadStatus.InProgress ||
                             _hotsweekUploadStatus == UploadStatus.Duplicate ||
                             _hotsweekUploadStatus == UploadStatus.Success;

            return needUpdate;
        }

        public bool Settled()
        {
            var ignored = new[] { UploadStatus.None, UploadStatus.UploadError, UploadStatus.InProgress };

            var hotsApiSettled = !App.CustomConfigurationSettings.AutoUploadReplayToHotslogs || !ignored.Contains(_hotsApiUploadStatus);

            var hotsweekSettled = !App.CustomConfigurationSettings.AutoUploadReplayToHotsweek || !ignored.Contains(_hotsweekUploadStatus);

            return hotsApiSettled || hotsweekSettled;
        }

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