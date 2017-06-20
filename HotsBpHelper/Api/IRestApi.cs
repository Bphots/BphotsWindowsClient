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

        List<ItemInfo> GetHeroList(string name);

        List<ItemInfo> GetMapList(string language);
    }
}