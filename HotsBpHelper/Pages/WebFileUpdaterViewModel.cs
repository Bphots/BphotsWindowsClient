using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Models;
using HotsBpHelper.Utils;
using RestSharp.Extensions;
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
            await GetFileList();
            await DownloadNeededFiles();
            CheckFiles();
            base.OnActivate();
        }

        private void CheckFiles()
        {
            if (FileUpdateInfos.Any(fui => fui.FileStatus == L("UpdateFailed")))
            {
                ShowMessageBox(L("FilesNotReady"),  MessageBoxButton.OK, MessageBoxImage.Exclamation);
                RequestClose(false);
            }
            RequestClose(true);
        }

        private async Task GetFileList()
        {
            List<RemoteFileInfo> remoteFileInfos;
            try
            {
                remoteFileInfos = await _restApi.GetRemoteFileListAsync();
            }
            catch (Exception)
            {
                ShowMessageBox(L("FilesNotReady"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                RequestClose(false);
                return;
            }
            FileUpdateInfos.AddRange(remoteFileInfos.Select(fi => new FileUpdateInfo
            {
                FileName = fi.Name,
                RemoteMD5 = fi.MD5,
                LocalFilePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, fi.Name.TrimStart('/')),
                FileStatus = L("Updating"),
            }));
        }

        private async Task DownloadNeededFiles()
        {
            await Task.Run(() =>
            {
                foreach (var fileUpdateInfo in FileUpdateInfos)
                {
                    if (NeedUpdate(fileUpdateInfo))
                    {
                        try
                        {
                            byte[] content = _restApi.DownloadFile(fileUpdateInfo.FileName);
                            content.SaveAs(fileUpdateInfo.LocalFilePath);
                            if (NeedUpdate(fileUpdateInfo)) fileUpdateInfo.FileStatus = L("UpdateFailed");
                            else fileUpdateInfo.FileStatus = L("UpToDate");
                            FileUpdateInfos.Refresh();
                        }
                        catch (Exception)
                        {
                            fileUpdateInfo.FileStatus = L("UpdateFailed");
                        }
                    }
                    else
                    {
                        fileUpdateInfo.FileStatus = L("UpToDate");
                    }
                }
            });
        }

        private bool NeedUpdate(FileUpdateInfo fileUpdateInfo)
        {
            if (!File.Exists(fileUpdateInfo.LocalFilePath))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(fileUpdateInfo.LocalFilePath));
                return true;
            }
            string localMd5 = FileUtils.CheckMD5(fileUpdateInfo.LocalFilePath).ToLower();
            string remoteMd5 = fileUpdateInfo.RemoteMD5.Trim().ToLower();
            return localMd5 != remoteMd5;
        }
    }
}