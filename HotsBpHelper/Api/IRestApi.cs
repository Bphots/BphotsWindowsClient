using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;

namespace HotsBpHelper.Api
{
    public interface IRestApi
    {
        Task<List<RemoteFileInfo>> GetRemoteFileListAsync();

        byte[] DownloadFile(string filePath);

        List<HeroInfo> GetHeroList(string name);

        List<MapInfo> GetMapList(string language);
    }
}