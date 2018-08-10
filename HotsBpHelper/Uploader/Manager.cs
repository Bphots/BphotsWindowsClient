using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private enum ProcessingStatus
        {
            None,
            Processed
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<ReplayFile, ProcessingStatus> _hotsApiProcessingQueue = new ConcurrentDictionary<ReplayFile, ProcessingStatus>();

        private readonly ConcurrentDictionary<ReplayFile, ProcessingStatus> _hotsweekProcessingQueue = new ConcurrentDictionary<ReplayFile, ProcessingStatus>();

        private readonly IRestApi _restApi;
        private UploadStrategy _uploadStrategy;

        private readonly IReplayStorage _storage;
        private Analyzer _analyzer;

        private static object StorageLock = new object();

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

        private bool UploadToHotsweek => App.CustomConfigurationSettings.AutoUploadReplayToHotsweek;

        private bool _uploadToHotsApi;

        private bool _uploadToHotsweek;

        public string HotsApiUploadStatus
        {
            get
            {
                if (!_hotsApiProcessingQueue.Any() || _hotsApiProcessingQueue.All(l => l.Value == ProcessingStatus.Processed))
                {
                    _hotsApiProcessingQueue.Clear();
                    return ViewModelBase.L("Idle");
                }

                var processed = _hotsApiProcessingQueue.Count(l => l.Value == ProcessingStatus.Processed);
                var remaining = _hotsApiProcessingQueue.Count - processed;

                if (SuspendUpload)
                    return ViewModelBase.L("Suspended") + @" " + remaining + @" " + ViewModelBase.L("Remaining");

                return ViewModelBase.L("Uploading") + @" " + remaining + @" " + ViewModelBase.L("Remaining");
            }
        }

        public string HotsweekUploadStatus
        {
            get
            {
                if (!_hotsweekProcessingQueue.Any() || _hotsweekProcessingQueue.All(l => l.Value == ProcessingStatus.Processed))
                {
                    _hotsweekProcessingQueue.Clear();
                    return ViewModelBase.L("Idle");
                }

                var processed = _hotsweekProcessingQueue.Count(l => l.Value == ProcessingStatus.Processed);
                var remaining = _hotsweekProcessingQueue.Count - processed;

                if (SuspendUpload)
                    return ViewModelBase.L("Suspended") + @" " + remaining + @" " + ViewModelBase.L("Remaining");

                return ViewModelBase.L("Uploading") + @" " + remaining + @" " + ViewModelBase.L("Remaining");
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
            Monitor.LatestReplayTime = replays.Max(l => l.Created);
            _uploadStrategy = App.CustomConfigurationSettings.UploadStrategy;
            _uploadToHotsApi = UploadToHotsApi;
            _uploadToHotsweek = UploadToHotsweek;
            if (App.CustomConfigurationSettings.UploadStrategy == UploadStrategy.UploadAll)
            {
                replays.Where(x => x.NeedHotsApiUpdate()).Reverse().Map(x => _hotsApiProcessingQueue[x] = ProcessingStatus.None);
                replays.Where(x => x.NeedHotsweekUpdate()).Reverse().Map(x => _hotsweekProcessingQueue[x] = ProcessingStatus.None);
            }

            _monitor.ReplayAdded += async (_, e) =>
            {
                await EnsureFileAvailable(e.Data, 3000);
                var replay = new ReplayFile(e.Data);
                Files.Insert(0, replay);

                _hotsApiProcessingQueue[replay] = ProcessingStatus.None;
                _hotsweekProcessingQueue[replay] = ProcessingStatus.None;

                OnReplayFileStatusChanged(new EventArgs<ReplayFile>(replay));
                OnStatusChanged();
            };

            _monitor.Start();

            _analyzer.MinimumBuild = Const.ReplayMinimumBuild;
            Task.Run(UploadHotsApiLoop).Forget();
            Task.Run(UploadHotsweekLoop).Forget();
        }

        public Monitor Monitor => _monitor;



        public void RepopulateQueue()
        {
            if (App.CustomConfigurationSettings.UploadStrategy != _uploadStrategy)
            {
                _hotsApiProcessingQueue.Clear();
                _hotsweekProcessingQueue.Clear();
                if (App.CustomConfigurationSettings.UploadStrategy == UploadStrategy.UploadAll)
                {
                    Files.Where(x => x.NeedHotsApiUpdate()).Reverse().Map(x => _hotsApiProcessingQueue[x] = ProcessingStatus.None);
                    Files.Where(x => x.NeedHotsweekUpdate()).Reverse().Map(x => _hotsweekProcessingQueue[x] = ProcessingStatus.None);
                }
            }
            else
            {
                if (_uploadToHotsApi != UploadToHotsApi)
                {
                    _hotsApiProcessingQueue.Clear();
                    Files.Where(x => x.NeedHotsApiUpdate()).Reverse().Map(x => _hotsApiProcessingQueue[x] = ProcessingStatus.None);
                }

                if (_uploadToHotsweek != UploadToHotsweek)
                {
                    _hotsweekProcessingQueue.Clear();
                    Files.Where(x => x.NeedHotsweekUpdate()).Reverse().Map(x => _hotsweekProcessingQueue[x] = ProcessingStatus.None);
                }
            }
            
            _uploadStrategy = App.CustomConfigurationSettings.UploadStrategy;
            _uploadToHotsApi = UploadToHotsApi;
            _uploadToHotsweek = UploadToHotsweek;
            OnStatusChanged();
        }

        private async Task UploadHotsApiLoop()
        {
            int invalidCount = 0;

            while (true)
            {
                await Task.Delay(1000);
                if (App.CustomConfigurationSettings.UploadStrategy != UploadStrategy.None && OcrUtil.InGame)
                    continue;

                try
                {
                    OnStatusChanged();
                    if (!UploadToHotsApi)
                        continue;

                    bool valid = true;
                    var replayFile = _hotsApiProcessingQueue.OrderByDescending(l => l.Key.Created).FirstOrDefault(l => l.Value == ProcessingStatus.None).Key;
                    if (replayFile == null)
                        continue;
                        
                    if (replayFile.NeedHotsApiUpdate())
                        replayFile.HotsApiUploadStatus = UploadStatus.InProgress;

                    bool parsed = false;
                    lock (replayFile)
                    {
                        parsed = _analyzer.Analyze(replayFile);
                    }
                    
                    if (!parsed || replayFile.HotsApiUploadStatus != UploadStatus.InProgress)
                    {
                        invalidCount ++;
                        if (_hotsApiProcessingQueue.ContainsKey(replayFile))
                            _hotsApiProcessingQueue[replayFile] = ProcessingStatus.Processed;

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
                    
                    await UploadHotsApi(replayFile);
                    
                    if (_hotsApiProcessingQueue.ContainsKey(replayFile))
                        _hotsApiProcessingQueue[replayFile] = ProcessingStatus.Processed;

                    OnReplayFileStatusChanged(new EventArgs<ReplayFile>(replayFile));

                    OnStatusChanged();
                    SaveReplayList();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in hotslogs upload loop");
                }
            }
        }

        private async Task UploadHotsweekLoop()
        {
            int invalidCount = 0;

            while (true)
            {
                await Task.Delay(App.HotsWeekDelay);

                if (!UploadToHotsweek)
                    continue;

                if (App.CustomConfigurationSettings.UploadStrategy != UploadStrategy.None && OcrUtil.InGame)
                    continue;

                OnStatusChanged();
                try
                {
                    bool valid = true;
                    var replayFile = _hotsweekProcessingQueue.OrderByDescending(l => l.Key.Created).FirstOrDefault(l => l.Value == ProcessingStatus.None).Key;
                    if (replayFile == null)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    if (replayFile.NeedHotsweekUpdate())
                        replayFile.HotsweekUploadStatus = UploadStatus.InProgress;

                    bool parsed = false;
                    lock (replayFile)
                    {
                        parsed = _analyzer.Analyze(replayFile);
                    }

                    if (!parsed || replayFile.HotsweekUploadStatus != UploadStatus.InProgress)
                    {
                        invalidCount++;
                        if (_hotsweekProcessingQueue.ContainsKey(replayFile))
                            _hotsweekProcessingQueue[replayFile] = ProcessingStatus.Processed;

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
                    
                    await UploadHotsBpHelper(replayFile);

                    if (_hotsweekProcessingQueue.ContainsKey(replayFile))
                        _hotsweekProcessingQueue[replayFile] = ProcessingStatus.Processed;

                    OnReplayFileStatusChanged(new EventArgs<ReplayFile>(replayFile));

                    OnStatusChanged();
                    SaveReplayList();
                }
                catch (Exception ex)
                {
                    await Task.Delay(3000);
                    Log.Error(ex, "Error in hotsweek upload loop");
                }
            }
        }

        private async Task UploadHotsApi(ReplayFile file)
        {
            await Task.Delay(1000);

            while (SuspendUpload)
            {
                await Task.Delay(1000);
            }

            if (file.HotsApiUploadStatus == UploadStatus.InProgress)
            {
                // if it is, upload it

                await _hotsApiUploader.Upload(file);
            }
        }

        private async Task UploadHotsBpHelper(ReplayFile file)
        {
            await _bpHelperUploader.CheckDuplicate(file);

            if (App.Debug)
                Log.Trace($"Pre-preparsing file {file.Filename} + {file.GameMode}");

            if (file.GameMode != GameMode.QuickMatch &&
                file.GameMode != GameMode.HeroLeague
                && file.GameMode != GameMode.TeamLeague &&
                file.GameMode != GameMode.UnrankedDraft)
            {
                file.HotsweekUploadStatus = UploadStatus.AiDetected;
            }
            // test if replay is eligible for upload (not AI, PTR, Custom, etc)
            Log.Trace($"Pre-parsing file {file.Filename} : { file.HotsweekUploadStatus }");
            
            while (SuspendUpload)
            {
                await Task.Delay(1000);
            }

            if (file.HotsweekUploadStatus == UploadStatus.InProgress)
            {
                // if it is, upload it

                await _bpHelperUploader.Upload(file);
            }
        }
        
        private void SaveReplayList()
        {
            try
            {
                // save only replays with fixed status. Will retry failed ones on next launch.
                lock (StorageLock)
                {
                    _storage.Save(Files.Where(x => x.Settled()));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving replay list");
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