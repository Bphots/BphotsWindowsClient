using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;

namespace HotsBpHelper.Api
{
    public interface IRestApi
    {
        Task<List<RemoteFileInfo>> GetRemoteFileListAsync(string url);

        byte[] DownloadFile(string filePath);

        void DownloadFileWithHander(string url, DownloadProgressChangedEventHandler downloadProgressChanged,
            DownloadDataCompletedEventHandler downloadCompleted);

        List<HeroInfo> GetHeroList(string name);

        List<MapInfo> GetMapList(string language);

        //String GetBroadcastInfo(string mode, string lang);

        List<BroadcastInfo> GetBroadcastInfo(string mode, string lang);

        Task<double> GetTimestamp();
    }
}