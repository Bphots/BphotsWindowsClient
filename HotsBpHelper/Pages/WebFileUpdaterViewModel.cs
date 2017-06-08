using System;
using System.IO;
using System.Linq;
using HotsBpHelper.Api;
using HotsBpHelper.Models;
using HotsBpHelper.Utils;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class WebFileUpdaterViewModel : ViewModelBase
    {
        private readonly IRestApi _restApi;

        public BindableCollection<FileUpdateInfo> FileUpdateInfos { get; set; } = new BindableCollection<FileUpdateInfo>();

        public WebFileUpdaterViewModel(IRestApi restApi)
        {
            _restApi = restApi;
        }

        protected override async void OnActivate()
        {
            var remoteFileInfo = await _restApi.GetRemoteFileListAsync();
            FileUpdateInfos.AddRange(remoteFileInfo.Select(fi => new FileUpdateInfo
            {
                FileName = fi.Name,
                RemoteMD5 = fi.MD5,
            }));
            foreach (var fileUpdateInfo in FileUpdateInfos)
            {
                if (NeedUpdate(fileUpdateInfo))
                {
                    fileUpdateInfo.FileStatus = L("Updating");
                }
                else
                {
                    fileUpdateInfo.FileStatus = L("UpToDate");
                }
            }
            base.OnActivate();
        }

        private bool NeedUpdate(FileUpdateInfo fileUpdateInfo)
        {
            var filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, fileUpdateInfo.FileName);
            if (!File.Exists(filePath)) return true;
            if (FileUtils.CheckMD5(filePath) != fileUpdateInfo.RemoteMD5.ToLower()) return true;
            return false;
        }
    }
}