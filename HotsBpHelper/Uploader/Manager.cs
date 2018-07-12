using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Heroes.ReplayParser;
using HotsBpHelper.Api;
using HotsBpHelper.Pages;
using HotsBpHelper.Settings;
using HotsBpHelper.Utils;
using NLog;

namespace HotsBpHelper.Uploader
{
    public class Manager
    {
        public enum ProcessingStatus
        {
            None,
            Processing,
            Processed
        }

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<ReplayFile, ProcessingStatus> _processingQueue = new ConcurrentDictionary<ReplayFile, ProcessingStatus>();

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

        public static bool SuspendUpload => ManualSuspend || IngameSuspend || InBpSuspend;

        public static bool ManualSuspend { get; set; }

        public static bool IngameSuspend { get; set; }

        public static bool InBpSuspend { get; set; }

        /// <summary>
        ///     Replay list
        /// </summary>
        public ObservableCollectionEx<ReplayFile> Files { get; } = new ObservableCollectionEx<ReplayFile>();
        
        private bool UploadToHotsApi => App.CustomConfigurationSettings.AutoUploadReplayToHotslogs;

        private bool UplaodToHotsweek => App.CustomConfigurationSettings.AutoUploadReplayToHotsweek;

        public string Status
        {
            get
            {
                if (!_processingQueue.Any() || _processingQueue.All(l => l.Value == ProcessingStatus.Processed))
                {
                    _processingQueue.Clear();
                    return ViewModelBase.L("Idle");
                }

                var processed = _processingQueue.Count(l => l.Value == ProcessingStatus.Processed);
                if (SuspendUpload)
                    return ViewModelBase.L("Suspended") + @" " + processed + "/" + _processingQueue.Count;

                return ViewModelBase.L("Uploading") + @" " + processed + "/" + _processingQueue.Count;
            }
        }

        /// <summary>
        ///     Fires when a new replay file is found
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        ///     Fires when a new replay file is found
        /// </summary>
        public event EventHandler<EventArgs<ReplayFile>> ReplayFileStatusChanged;

        /// <summary>
        ///     Start uploading and watching for new replays
        /// </summary>
        public void Start()
        {
            if (_initialized)
                return;

            _initialized = true;

            _bpHelperUploader = new BpHelperUploader(_restApi);
            _hotsApiUploader = new HotsApiUploader();

            _analyzer = new Analyzer();
            _monitor = new Monitor();

            var replays = ScanReplays();
            Files.AddRange(replays.OrderByDescending(l => l.Created));
            if (App.CustomConfigurationSettings.UploadStrategy == UploadStrategy.UploadAll)
                replays.Where(x => x.NeedUpdate()).Reverse().Map(x => _processingQueue[x] = ProcessingStatus.None);

            _monitor.ReplayAdded += async (_, e) =>
            {
                await EnsureFileAvailable(e.Data, 3000);
                var replay = new ReplayFile(e.Data);
                Files.Insert(0, replay);
                _processingQueue[replay] = ProcessingStatus.None;
                OnReplayFileStatusChanged(new EventArgs<ReplayFile>(replay));
                OnStatusChanged();
            };

            _monitor.Start();

            _analyzer.MinimumBuild = Const.ReplayMinimumBuild;
            Task.Run(UploadLoop).Forget();
        }

        public Monitor Monitor => _monitor;

        public void RepopulateQueue()
        {
            _processingQueue.Clear();
            if (App.CustomConfigurationSettings.UploadStrategy == UploadStrategy.UploadAll)
                Files.Where(x => x.NeedUpdate()).Reverse().Map(x => _processingQueue[x] = ProcessingStatus.None);

            OnStatusChanged();
        }

        private async Task UploadLoop()
        {
            int invalidCount = 0;

            while (true)
            {
                await Task.Delay(3000);
                if (App.CustomConfigurationSettings.UploadStrategy != UploadStrategy.None && OcrUtil.InGame)
                    continue;

                try
                {
                    OnStatusChanged();
                    bool valid = true;
                    var replayFile = _processingQueue.OrderByDescending(l => l.Key.Created).FirstOrDefault(l => l.Value == ProcessingStatus.None).Key;
                    if (replayFile == null)
                        continue;
                        
                    if (UplaodToHotsweek && (replayFile.HotsweekUploadStatus == UploadStatus.None || replayFile.HotsweekUploadStatus == UploadStatus.Reserved))
                        replayFile.HotsweekUploadStatus = UploadStatus.InProgress;
                    if (UploadToHotsApi && replayFile.HotsApiUploadStatus == UploadStatus.None)
                        replayFile.HotsApiUploadStatus = UploadStatus.InProgress;

                    var replay = _analyzer.Analyze(replayFile);
                    if (replay != null && (replayFile.HotsweekUploadStatus == UploadStatus.InProgress ||
                                            replayFile.HotsApiUploadStatus == UploadStatus.InProgress))
                    {
                        if (_processingQueue.ContainsKey(replayFile))
                            _processingQueue[replayFile] = ProcessingStatus.Processing;
                    }
                    else
                    {
                        invalidCount ++;
                        if (_processingQueue.ContainsKey(replayFile))
                            _processingQueue[replayFile] = ProcessingStatus.Processed;

                        valid = false;
                    }

                    if (invalidCount == 10)
                    {
                        invalidCount = 0;
                        SaveReplayList();
                    }

                    OnReplayFileStatusChanged(new EventArgs<ReplayFile>(replayFile));

                    if (!valid)
                        continue;

                    if (UploadToHotsApi)
                        await UploadHotsApi(replayFile);

                    if (UplaodToHotsweek)
                        await UploadHotsBpHelper(replayFile, replay);
                    
                    if (_processingQueue.ContainsKey(replayFile))
                        _processingQueue[replayFile] = ProcessingStatus.Processed;

                    OnReplayFileStatusChanged(new EventArgs<ReplayFile>(replayFile));

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
            await Task.Delay(1000);

            if (file.HotsApiUploadStatus == UploadStatus.InProgress)
            {
                // if it is, upload it
                while (SuspendUpload)
                {
                    await Task.Delay(1000);
                }

                await _hotsApiUploader.Upload(file);
            }
        }

        private async Task UploadHotsBpHelper(ReplayFile file, Replay replay)
        {
            await _bpHelperUploader.CheckDuplicate(file);
            if (App.Debug)
                _log.Trace($"Pre-preparsing file {file.Filename} + {replay.GameMode}");

            if (replay != null && replay.GameMode != GameMode.QuickMatch &&
                replay.GameMode != GameMode.HeroLeague
                && replay.GameMode != GameMode.TeamLeague &&
                replay.GameMode != GameMode.UnrankedDraft)
            {
                file.HotsweekUploadStatus = UploadStatus.AiDetected;
            }

            await Task.Delay(1000);
            // test if replay is eligible for upload (not AI, PTR, Custom, etc)
            _log.Trace($"Pre-parsing file {file.Filename} : { file.HotsweekUploadStatus }");
            if (file.HotsweekUploadStatus == UploadStatus.InProgress)
            {
                // if it is, upload it
                while (SuspendUpload)
                {
                    await Task.Delay(1000);
                }

                await _bpHelperUploader.Upload(file);
            }
        }

        //private void RefreshStatusAndAggregates()
        //{
        //    Status =
        //        Files.Any(
        //            x =>
        //                x.HotsweekUploadStatus == UploadStatus.InProgress ||
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


        protected virtual void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnReplayFileStatusChanged(EventArgs<ReplayFile> e)
        {
            ReplayFileStatusChanged?.Invoke(this, e);
        }
    }
}