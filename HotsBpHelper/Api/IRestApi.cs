using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Uploader;

namespace HotsBpHelper.Api
{
    public interface IRestApi
    {
        List<BroadcastInfo> GetBroadcastInfo(string mode, string lang);

        string GetOss();

        Task<List<RemoteFileInfo>> GetRemoteFileListAsync(string url);

        void DownloadFileAsync(string url, DownloadProgressChangedEventHandler downloadProgressChanged,
            DownloadDataCompletedEventHandler downloadCompleted);

        Task<List<LobbyHeroInfo>> GetLobbyHeroList(string name);

        Task<List<LobbyMapInfo>> GetLobbyMapList(string name);

        Task<double> GetTimestamp();

        Task<FingerPrintStatusCollection> CheckDuplicatesAsync(IEnumerable<ReplayIdentity> replayIdentities);

        Task<int> GetMinimalBuild();

        Task<object> Analysis(string type, string para, string lang);

        Task<UploadStatus> UploadReplay(string file, string fingerprint);

        Task<UploadStatus> UploadImage(string file, string id);

        Task<Dictionary<int, HeroInfoV2>> GetHeroListV2();

        Task<Dictionary<string, MapInfoV2>> GetMapListV2();
    }
}