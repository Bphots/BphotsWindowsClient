using System;
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
using NLog;

namespace HotsBpHelper.Uploader
{
    public class Manager : INotifyPropertyChanged
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly List<ReplayFile> _processingQueue = new List<ReplayFile>(new ConcurrentStack<ReplayFile>());

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
            Files.ItemPropertyChanged += (_, __) => { RefreshStatusAndAggregates(); };
            Files.CollectionChanged += (_, __) => { RefreshStatusAndAggregates(); };
        }

        /// <summary>
        ///     Replay list
        /// </summary>
        public ObservableCollectionEx<ReplayFile> Files { get; } = new ObservableCollectionEx<ReplayFile>();

        /// <summary>
        ///     Current uploader status
        /// </summary>
        public string Status { get; private set; } = "";

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
            replays.Where(x => x.NeedUpdate()).Reverse().Map(x => _processingQueue.Add(x));

            _monitor.ReplayAdded += async (_, e) =>
            {
                await EnsureFileAvailable(e.Data, 3000);
                var replay = new ReplayFile(e.Data);
                Files.Insert(0, replay);
                _processingQueue.Add(replay);
            };
            _monitor.Start();

            _analyzer.MinimumBuild = await _bpHelperUploader.GetMinimumBuild();
            Task.Run(UploadLoop).Forget();
        }

        private async Task UploadLoop()
        {
            while (true)
            {
                try
                {
                    var files = new Dictionary<ReplayFile, Replay>();

                    for (var i = 0; i < 10 && _processingQueue.Any();)
                    {
                        var file = _processingQueue[0];
                        
                        file.BpHelperUploadStatus = UploadStatus.InProgress;
                        file.HotsApiUploadStatus = UploadStatus.InProgress;

                        var replay = _analyzer.Analyze(file);
                        if (file.BpHelperUploadStatus == UploadStatus.InProgress || file.HotsApiUploadStatus == UploadStatus.InProgress)
                        {
                            files[file] = replay;
                            ++i;
                        }
                        _processingQueue.RemoveAt(0);
                    }

                    await UploadHotsBpHelper(files);

                    await UploadHotsApi(files);

                    SaveReplayList();
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Error in upload loop");
                }
            }
        }

        private async Task UploadHotsApi(Dictionary<ReplayFile, Replay> files)
        {
            foreach (var file in files)
            {
                if (file.Key.HotsApiUploadStatus == UploadStatus.InProgress)
                {
                    // if it is, upload it
                    await _hotsApiUploader.Upload(file.Key);
                }
            }
        }

        private async Task UploadHotsBpHelper(Dictionary<ReplayFile, Replay> files)
        {
            await _bpHelperUploader.CheckDuplicate(files.Keys.ToList());

            foreach (var file in files)
            {
                // test if replay is eligible for upload (not AI, PTR, Custom, etc)
                if (file.Key.BpHelperUploadStatus == UploadStatus.InProgress)
                {
                    // if it is, upload it
                    await _bpHelperUploader.Upload(file.Key);
                }
            }
        }

        private void RefreshStatusAndAggregates()
        {
            Status =
                Files.Any(
                    x =>
                        x.BpHelperUploadStatus == UploadStatus.InProgress ||
                        x.HotsApiUploadStatus == UploadStatus.InProgress)
                    ? "Uploading..."
                    : "Idle";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }

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
    }
}