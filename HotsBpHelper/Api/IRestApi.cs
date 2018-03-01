using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Uploader;

namespace HotsBpHelper.Api
{
    public interface IRestApi
    {
        Task<List<RemoteFileInfo>> GetRemoteFileListAsync(string url);

        byte[] DownloadFile(string filePath);

        void DownloadFileAsync(string url, DownloadProgressChangedEventHandler downloadProgressChanged,
            DownloadDataCompletedEventHandler downloadCompleted);
        
        List<LobbyHeroInfo> GetLobbyHeroList(string name);

        //String GetBroadcastInfo(string mode, string lang);

        List<BroadcastInfo> GetBroadcastInfo(string mode, string lang);

        Task<double> GetTimestamp();

        Task<FingerPrintStatusCollection> CheckDuplicatesAsync(IEnumerable<ReplayIdentity> replayIdentities);

        Task<int> GetMinimalBuild();

        Task<UploadStatus> UploadReplay(string file);

        Dictionary<int, HeroInfoV2> GetHeroListV2();

        Dictionary<string, MapInfoV2> GetMapListV2();
    }
}