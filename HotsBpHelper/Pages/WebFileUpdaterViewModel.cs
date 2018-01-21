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
using HotsBpHelper.Services;
using HotsBpHelper.Utils;
using RestSharp.Extensions;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class WebFileUpdaterViewModel : ViewModelBase
    {
        private readonly IRestApi _restApi;
        private readonly IToastService _toastService;

        //private int percent,count=0, lastBalloon = 0;

        public BindableCollection<FileUpdateInfo> FileUpdateInfos { get; set; } = new BindableCollection<FileUpdateInfo>();

        public WebFileUpdaterViewModel(IRestApi restApi, IToastService toastSevice)
        {
            _restApi = restApi;
            _toastService = toastSevice;
        }

        private void ReceiveBroadcast()
        {
        //接收公告，并以对话框的形式显示
            var BroadcastList = _restApi.GetBroadcastInfo("0", App.Language);
            if (BroadcastList != null)
            {
                //MessageBox.Show("1");
                foreach (Api.Model.BroadcastInfo broadcast in BroadcastList)
                {
                    if (broadcast.Type == 0)
                    {
                        BroadcastWindow b = new BroadcastWindow(broadcast.Msg, broadcast.Url);
                        b.Show();
                    }
                    
                }
                foreach (Api.Model.BroadcastInfo broadcast in BroadcastList)
                {
                    if (broadcast.Type == 1)
                    {
                        ErrorView e = new ErrorView(ViewModelBase.L("Reminder"), broadcast.Msg, broadcast.Url);
                        //e.isShutDown = false;
                        e.ShowDialog();
                        //ShowMessageBox(broadcast.Msg+"\n"+ broadcast.Url, MessageBoxButton.OK, MessageBoxImage.Warning);
                        //e.Pause();
                    }
                }
                
            }
        }

        protected override async void OnViewLoaded()
        {
            base.OnViewLoaded();
            ReceiveBroadcast();
            await GetFileList();
            await DownloadNeededFiles();
            CheckFiles();
        }

        private void CheckFiles()
        {
            try
            {
                if (FileUpdateInfos.Any(fui => fui.FileStatus == L("UpdateFailed")))
                {
                    ErrorView _errorView = new ErrorView(L("FileUpdateFail"), L("FilesNotReady"), "https://www.bphots.com/articles/errors/");
                    _errorView.ShowDialog();
                    //ShowMessageBox(L("FilesNotReady"),  MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    RequestClose(false);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                ErrorView _errorView = new ErrorView(L("FilesNotReady"), e.Message, "https://www.bphots.com/articles/errors/");
                RequestClose(false);
                return;
            }
            RequestClose(true);
        }

        private async Task GetFileList()
        {
            List<RemoteFileInfo> remoteFileInfos;
            try
            {
                remoteFileInfos = await _restApi.GetRemoteFileListAsync();
                Logger.Trace("Remote files:\r\n{0}", string.Join("\r\n", remoteFileInfos.Select(rfi => rfi.Name)));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                ErrorView _errorView = new ErrorView(L("FilesNotReady"), e.Message, "https://www.bphots.com/articles/errors/");
                //ShowMessageBox(L("FilesNotReady"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                RequestClose(false);
                return;
            }
            FileUpdateInfos.AddRange(remoteFileInfos.Select(fi => new FileUpdateInfo
            {
                FileName = fi.Name,
                Url = fi.Url,
                RemoteMD5 = fi.MD5,
                LocalFilePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, fi.Name.TrimStart('/')),
                Path=fi.Url.Remove(0,24),//移去https://static.bphots.com/
                FileStatus = L("Updating"),
            }));
        }

        private async Task DownloadNeededFiles()
        {
            Execute.OnUIThread(() => _toastService.ShowInformation(L("UpdateFullText") + Environment.NewLine + L("HotsBpHelper")));
            await Task.Run(() =>
            {
                
                foreach (var fileUpdateInfo in FileUpdateInfos)
                {
                    if (NeedUpdate(fileUpdateInfo))
                    {
                        try
                        {
                            Logger.Trace("Downloading file: {0}", fileUpdateInfo.FileName);

                            //防盗链算法
                            string T = ((int)DateTime.Now.AddMinutes(1).ToUnixTimestamp()).ToString("x8").ToLower();
                            string S=Api.Security.SecurityProvider.UrlKey + System.Web.HttpUtility.UrlEncode(fileUpdateInfo.Path).Replace("%2f","/") + T;
                            string SIGN = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(S, "MD5").ToLower();

                            byte[] content = _restApi.DownloadFile(fileUpdateInfo.Url + "?sign=" + SIGN + "&t=" + T);
                            //byte[] content = _restApi.DownloadFile(fileUpdateInfo.Url);
                            content.SaveAs(fileUpdateInfo.LocalFilePath);
                            Logger.Trace("Downloaded. Bytes count: {0}", content.Length);
                            if (NeedUpdate(fileUpdateInfo)) fileUpdateInfo.FileStatus = L("UpdateFailed");
                            else fileUpdateInfo.FileStatus = L("UpToDate");
                            Logger.Trace("File status: {0}", fileUpdateInfo.FileStatus);
                            FileUpdateInfos.Refresh();
                        }
                        catch (Exception e)
                        {
                            fileUpdateInfo.FileStatus = L("UpdateFailed")+e.Message;
                            Logger.Error(e, "Downloading error.");
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
            string localMd5 = Md5Util.CaculateFileMd5(fileUpdateInfo.LocalFilePath).ToLower();
            string remoteMd5 = fileUpdateInfo.RemoteMD5.Trim().ToLower();
            return localMd5 != remoteMd5;
        }
    }
}