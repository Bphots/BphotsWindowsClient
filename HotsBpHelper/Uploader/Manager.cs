﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Heroes.ReplayParser;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Security;
using HotsBpHelper.Settings;
using HotsBpHelper.Utils;
using NLog;

namespace HotsBpHelper.Uploader
{
    public class Manager : INotifyPropertyChanged
    {
        public static bool SuspendUpload { get; set; }

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<ReplayFile, bool> _processingQueue = new ConcurrentDictionary<ReplayFile, bool>();

        private readonly IRestApi _restApi;
        
        private readonly IReplayStorage _storage;
        private Analyzer _analyzer;

        private BpHelperUploader _bpHelperUploader;
        private HotsApiUploader _hotsApiUploader;
        private bool _initialized;
        private Monitor _monitor;

        public Manager(IReplayStorage storage, IRestApi restApi)
        {
            _storage = storage;
            _restApi = restApi;
            Files.ItemPropertyChanged += (_, __) => { OnStatusChanged(); };
            Files.CollectionChanged += (_, __) => { OnStatusChanged(); };
        }

        /// <summary>
        /// Fires when a new replay file is found
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        ///     Replay list
        /// </summary>
        public ObservableCollectionEx<ReplayFile> Files { get; } = new ObservableCollectionEx<ReplayFile>();
        
        /// <summary>
        ///     Whether to mark replays for upload to hotslogs
        /// </summary>
        public bool UploadToHotslogs
        {
            get { return _hotsApiUploader?.UploadToHotslogs ?? false; }
            set
            {
                if (_hotsApiUploader != null)
                {
                    _hotsApiUploader.UploadToHotslogs = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Start uploading and watching for new replays
        /// </summary>
        public async void Start()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            _bpHelperUploader = new BpHelperUploader(_restApi);
            _hotsApiUploader = new HotsApiUploader();

            _analyzer = new Analyzer();
            _monitor = new Monitor();

            var replays = ScanReplays();
            Files.AddRange(replays);
            replays.Where(x => x.NeedUpdate()).Reverse().Map(x => _processingQueue[x] = false );

            _monitor.ReplayAdded += async (_, e) =>
            {
                await EnsureFileAvailable(e.Data, 3000);
                var replay = new ReplayFile(e.Data);
                Files.Insert(0, replay);
                _processingQueue[replay] = false;
            };
            
            _monitor.Start();

            _analyzer.MinimumBuild = await _bpHelperUploader.GetMinimumBuild();
            Task.Run(UploadLoop).Forget();
        }

        private void RepopulateQueue()
        {
            _processingQueue.Clear();
            Files.Where(x => x.NeedUpdate()).Reverse().Map(x => _processingQueue[x] = false);
        }

        private bool UploadToHotsApi => App.CustomConfigurationSettings.AutoUploadNewReplayToHotslogs;

        private bool UplaodToHotsWeek => App.CustomConfigurationSettings.AutoUploadNewReplayToHotsweek;
        
        private async Task UploadLoop()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (App.CustomConfigurationSettings.UploadStrategy != UploadStrategy.None && OcrUtil.InGame)
                    continue;

                try
                {
                    OnStatusChanged();
                    var files = new Dictionary<ReplayFile, Replay>();

                    for (var i = 0; i < 10 && _processingQueue.Any();)
                    {
                        ReplayFile file =_processingQueue.FirstOrDefault(l => !l.Value).Key;
                        if (file == null)
                            continue;
                        
                        
                        if (UplaodToHotsWeek)
                            file.BpHelperUploadStatus = UploadStatus.InProgress;
                        if (UploadToHotsApi)
                            file.HotsApiUploadStatus = UploadStatus.InProgress;

                        var replay = _analyzer.Analyze(file);
                        if (file.BpHelperUploadStatus == UploadStatus.InProgress || file.HotsApiUploadStatus == UploadStatus.InProgress)
                        {
                            files[file] = replay;
                            ++i;
                        }
                    }

                    if (UplaodToHotsWeek)
                        await _bpHelperUploader.CheckDuplicate(files.Keys.ToList());

                    foreach (var file in files)
                    {
                        if (UploadToHotsApi)
                            await UploadHotsApi(file.Key);

                        if (UplaodToHotsWeek)
                            await UploadHotsBpHelper(file.Key);

                        _processingQueue[file.Key] = true;
                    }
                    
                    OnStatusChanged();
                    SaveReplayList();
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Error in upload loop");
                }
            }
        }

        private async Task UploadHotsApi(ReplayFile file)
        {
            if (file.HotsApiUploadStatus == UploadStatus.InProgress)
            {
                // if it is, upload it
                if (!SuspendUpload)
                    await _hotsApiUploader.Upload(file);
            }
        }

        private async Task UploadHotsBpHelper(ReplayFile file)
        {
            // test if replay is eligible for upload (not AI, PTR, Custom, etc)
            if (file.BpHelperUploadStatus == UploadStatus.InProgress)
            {
                // if it is, upload it
                if (!SuspendUpload)
                    await _bpHelperUploader.Upload(file);
            }
        }

        //private void RefreshStatusAndAggregates()
        //{
        //    Status =
        //        Files.Any(
        //            x =>
        //                x.BpHelperUploadStatus == UploadStatus.InProgress ||
        //                x.HotsApiUploadStatus == UploadStatus.InProgress)
        //            ? "Uploading..."
        //            : "Idle";
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        //}

        private void SaveReplayList()
        {
            try
            {
                // save only replays with fixed status. Will retry failed ones on next launch.
                _storage.Save(Files.Where(x => x.Settled()));
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error saving replay list");
            }
        }

        /// <summary>
        ///     Load replay cache and merge it with folder scan results
        /// </summary>
        private List<ReplayFile> ScanReplays()
        {
            var replays = new List<ReplayFile>(_storage.Load());
            var lookup = new HashSet<ReplayFile>(replays);
            var comparer = new ReplayFile.ReplayFileComparer();
            replays.AddRange(
                _monitor.ScanReplays().Select(x => new ReplayFile(x)).Where(x => !lookup.Contains(x, comparer)));
            return replays.OrderByDescending(x => x.Created).ToList();
        }

        /// <summary>
        ///     Ensure that HotS client finished writing replay file and it can be safely open
        /// </summary>
        /// <param name="filename">Filename to test</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <param name="testWrite">Whether to test read or write access</param>
        public async Task EnsureFileAvailable(string filename, int timeout, bool testWrite = true)
        {
            var timer = new Stopwatch();
            timer.Start();
            while (timer.ElapsedMilliseconds < timeout)
            {
                try
                {
                    if (testWrite)
                    {
                        File.OpenWrite(filename).Close();
                    }
                    else
                    {
                        File.OpenRead(filename).Close();
                    }
                    return;
                }
                catch (IOException)
                {
                    // File is still in use
                    await Task.Delay(100);
                }
                catch
                {
                    return;
                }
            }
        }

        public string Status
        {
            get
            {
                if (SuspendUpload)
                    return "Suspended";

                if (!_processingQueue.Any() || _processingQueue.All(l => l.Value))
                    return "Idle";

                int processed = _processingQueue.Count(l => l.Value);
                return @"Uploading... " + processed + "/" + _processingQueue.Count;
            }
        }


        protected virtual void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}