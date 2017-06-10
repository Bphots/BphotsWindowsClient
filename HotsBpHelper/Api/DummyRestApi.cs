using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;

namespace HotsBpHelper.Api
{
    public class DummyRestApi:IRestApi
    {
        public Task<List<RemoteFileInfo>> GetRemoteFileListAsync()
        {
            var result = new List<RemoteFileInfo>()
            {
                new RemoteFileInfo()
                {
                    Name = "vue.min.js",
                    MD5 = "9a‌​a6‌​e5‌​f2‌​25‌​6c‌​17‌​d2‌​d4‌​30‌​b1‌​00‌​03‌​2b‌​99‌​7c",
                },
                new RemoteFileInfo()
                {
                    Name = "advice.json",
                    MD5 = "8e2e7e0c0d6d5c0d786018b77e99f41d",
                },
                new RemoteFileInfo()
                {
                    Name = "index.html",
                    MD5 = "92e3f5db47853429cec4918b654f1d0",
                },
            };
            return Task.FromResult(result);
        }

        public byte[] DownloadFile(string filePath)
        {
            throw new System.NotImplementedException();
        }
    }
}