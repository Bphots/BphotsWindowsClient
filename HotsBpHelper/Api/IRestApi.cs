using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Uploader;
using ImageProcessor.HashProcessing;
using LobbyFileParser;

namespace HotsBpHelper.Api
{
    public interface IRestApi
    {
        List<BroadcastInfo> GetBroadcastInfo(string mode, string lang);

        Task<LobbyParameter> GetLobbyParameter(string region);

        string GetOss();

        Task<List<RemoteFileInfo>> GetRemoteFileListAsync(string url);

        void DownloadFileAsync(string url, DownloadProgressChangedEventHandler downloadProgressChanged,
            DownloadDataCompletedEventHandler downloadCompleted);

        Task<List<EachHero>> GetHashList();

        Task<List<LobbyHeroInfo>> GetLobbyHeroList(string region);

        Task<List<LobbyMapInfo>> GetLobbyMapList(string region);

        Task<double> GetTimestamp();

        Task<FingerPrintStatusCollection> CheckDuplicatesAsync(IEnumerable<ReplayIdentity> replayIdentities);

        Task<FingerPrintStatusCollection> CheckDuplicatesV2(ReplayIdentity replayIdentity);

        Task<int> GetMinimalBuild();

        Task<object> Analyze(string type, string para, string lang);

        Task<UploadStatus> UploadReplayJson(string file, string fingerprint);

        Task<UploadStatus> UploadReplay(string file, string fingerprint);

        Task<UploadStatus> UploadImage(string file, string id);

        Task<Dictionary<int, HeroInfoV2>> GetHeroListV2();

        Task<Dictionary<string, MapInfoV2>> GetMapListV2();
    }
}