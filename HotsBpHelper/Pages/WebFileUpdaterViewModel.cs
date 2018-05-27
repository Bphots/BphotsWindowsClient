using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Windows;
using DotNetHelper;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Api.Security;
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

        private bool _hasDisplayedMessage;

        private string _localDir;
        private string _remoteUrl;
        private Visibility _visibility;

        private int _currentIndex = -1;
        private double _downloadedBytes;
        
        public WebFileUpdaterViewModel(IRestApi restApi, IToastService toastSevice)
        {
            _restApi = restApi;
            _toastService = toastSevice;
            _localDir = Const.LOCAL_WEB_FILE_DIR;
            _remoteUrl = "get/filelist";
            _visibility = Visibility.Hidden;
        }

        public BindableCollection<FileUpdateInfo> FileUpdateInfos { get; set; } =
            new BindableCollection<FileUpdateInfo>();

        public Visibility Visibility
        {
            get { return _visibility; }
            set { SetAndNotify(ref _visibility, value); }
        }

        public ShellViewModel ShellViewModel { get; set; }

        public event EventHandler UpdateCompleted;

        public void ProcessPostDownload()
        {
            foreach (var fileUpdateInfo in FileUpdateInfos.Where(f => f.LocalFilePath.EndsWith(@".zip")))
            {
                FilePath localPath = fileUpdateInfo.LocalFilePath;
                if (!localPath.Exists())
                    continue;

                using (var archive = ZipFile.OpenRead(localPath))
                {
                    ExtractToDirectory(archive, localPath.GetDirPath(), true);
                }

                if (localPath.GetFileExt() == ".zip")
                {
                    FilePath zipMd5Path = localPath.GetDirPath() + localPath.GetFileNameWithoutExtension() + @".md5";
                    File.WriteAllText(zipMd5Path, fileUpdateInfo.RemoteMD5);
                }

                localPath.DeleteIfExists();
            }

            OnUpdateCompleted();
        }

        private static void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (var file in archive.Entries)
            {
                var completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                if (file.Name == "")
                {
                    // Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }

        public void SetPaths(string localDir, string remoteUrl)
        {
            _localDir = localDir;
            _remoteUrl = remoteUrl;
        }

      
        protected override async void OnViewLoaded()
        {
            base.OnViewLoaded();
            await GetFileList();
            Execute.OnUIThread(DownloadNextItem);
        }

        private void CheckFiles()
        {
            try
            {
                if (FileUpdateInfos.Any(fui => fui.FileStatus == L("UpdateFailed")))
                {
                    var _errorView = new ErrorView(L("FileUpdateFail"), L("FilesNotReady"),
                        "https://www.bphots.com/articles/errors/");
                    _errorView.ShowDialog();
                    //ShowMessageBox(L("FilesNotReady"),  MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    RequestClose(false);
                }
            }
            catch (InvalidOperationException e)
            {
                Logger.Error(e);
                var errorView = new ErrorView(L("FileUpdateFail"), e.Message, "https://www.bphots.com/articles/errors/1");
                errorView.ShowDialog();
                RequestClose(false);
                return;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                var errorView = new ErrorView(L("FilesNotReady"), e.Message, "https://www.bphots.com/articles/errors/");
                errorView.ShowDialog();
                RequestClose(false);
                return;
            }
            RequestClose(true);
        }

        private long _totalBytes = 0;

        private async Task GetFileList()
        {
            List<RemoteFileInfo> remoteFileInfos;
            try
            {
                remoteFileInfos = await _restApi.GetRemoteFileListAsync(_remoteUrl);
                //Logger.Trace("Remote files:\r\n{0}", string.Join("\r\n", remoteFileInfos.Select(rfi => rfi.Name)));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                var _errorView = new ErrorView(L("FilesNotReady"), e.Message, "https://www.bphots.com/articles/errors/");
                //ShowMessageBox(L("FilesNotReady"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                RequestClose(false);
                return;
            }

            remoteFileInfos.ForEach(r => _totalBytes += long.Parse(r.Size));
            FileUpdateInfos.AddRange(remoteFileInfos.Select(fi => new FileUpdateInfo
            {
                FileName = fi.Name,
                Url = fi.Url,
                RemoteMD5 = fi.MD5,
                LocalFilePath = Path.Combine(App.AppPath, _localDir, fi.Name.TrimStart('/')),
                Path = fi.Url.Remove(0, 24), //移去https://static.bphots.com/
                FileStatus = L("Updating")
            }));
        }

        private void DownloadNextItem()
        {
            _currentIndex++;
            if (_currentIndex >= FileUpdateInfos.Count)
            {
                CheckFiles();
                ProcessPostDownload();
                return;
            }

            var fileUpdateInfo = FileUpdateInfos[_currentIndex];
            if (NeedUpdate(fileUpdateInfo))
            {
                if (!_hasDisplayedMessage)
                {
                    _hasDisplayedMessage = true;
                    _toastService.CloseMessages(L("Loading"));
                    _toastService.ShowInformation(L("UpdateFullText") + Environment.NewLine + L("HotsBpHelper"));
                }

                try
                {
                    Logger.Trace("Downloading file: {0}", fileUpdateInfo.FileName);

                    //防盗链算法
                    var T = ((int) DateTime.Now.AddMinutes(1).ToUnixTimestamp()).ToString("x8").ToLower();
                    var S = SecurityProvider.UrlKey + HttpUtility.UrlEncode(fileUpdateInfo.Path).Replace("%2f", "/") + T;
                    string SIGN = string.Empty;
                    try
                    {
                        SIGN = GetSwcMD5(S).ToLower();
                    }
                    catch (Exception)
                    {
#pragma warning disable 618
                        SIGN = FormsAuthentication.HashPasswordForStoringInConfigFile(S, "MD5").ToLower();
#pragma warning restore 618
                    }
                    
                    _restApi.DownloadFileAsync(fileUpdateInfo.Url + "?sign=" + SIGN + "&t=" + T,
                        DownloadProgressChanged, DownloadFileCompleted);
                }
                catch (Exception e)
                {
                    fileUpdateInfo.FileStatus = L("UpdateFailed") + e.Message;
                    Logger.Error(e, "Downloading error.");
                }
            }
            else
            {
                fileUpdateInfo.FileStatus = L("UpToDate");
                // ReSharper disable once TailRecursiveCall
                DownloadNextItem();
            }
        }

        private static string GetSwcMD5(string value)
        {
            var algorithm = MD5.Create();
            byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            string sh1 = "";
            for (int i = 0; i < data.Length; i++)
            {
                sh1 += data[i].ToString("x2").ToUpperInvariant();
            }

            return sh1;
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var bytesIn = double.Parse(e.BytesReceived.ToString());
            var currentBytes = _downloadedBytes + bytesIn;
            var percentage = (int) (currentBytes / _totalBytes * 100);
            ShellViewModel.PercentageInfo = L("Updating") + @" " + percentage + @"%";
        }

        private void DownloadFileCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            var webClient = (WebClient) sender;
            _downloadedBytes += e.Result.Length;
            var fileUpdateInfo = FileUpdateInfos[_currentIndex];
            var content = e.Result;

            content.SaveAs(fileUpdateInfo.LocalFilePath);

            FilePath localPath = fileUpdateInfo.LocalFilePath;

            Logger.Trace("Downloaded. Bytes count: {0}", content.Length);
            if (localPath.GetFileExt() == ".zip" && localPath.Exists() || !NeedUpdate(fileUpdateInfo))
                fileUpdateInfo.FileStatus = L("UpToDate");
            else
                fileUpdateInfo.FileStatus = L("UpdateFailed");
            Logger.Trace("File status: {0}", fileUpdateInfo.FileStatus);
            FileUpdateInfos.Refresh();

            webClient.Dispose();

            DownloadNextItem();
        }

        private bool NeedUpdate(FileUpdateInfo fileUpdateInfo)
        {
            FilePath localPath = fileUpdateInfo.LocalFilePath;
            if (localPath.GetFileExt() == @".zip")
            {
                FilePath zipMd5Path = localPath.GetDirPath() + localPath.GetFileNameWithoutExtension() + @".md5";
                if (!File.Exists(zipMd5Path))
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Directory.CreateDirectory(Path.GetDirectoryName(fileUpdateInfo.LocalFilePath));
                    return true;
                }

                var localZipMd5 = zipMd5Path.ReadAsString();
                var remoteZipMd5 = fileUpdateInfo.RemoteMD5.Trim().ToLower();
                return localZipMd5 != remoteZipMd5;
            }

            if (!File.Exists(fileUpdateInfo.LocalFilePath))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(fileUpdateInfo.LocalFilePath));
                return true;
            }

            var localMd5 = Md5Util.CaculateFileMd5(fileUpdateInfo.LocalFilePath).ToLower();
            var remoteMd5 = fileUpdateInfo.RemoteMD5.Trim().ToLower();
            return localMd5 != remoteMd5;
        }

        protected virtual void OnUpdateCompleted()
        {
            UpdateCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}