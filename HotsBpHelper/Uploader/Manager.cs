using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Heroes.ReplayParser;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Security;
using NLog;

namespace HotsBpHelper.Uploader
{
    public class Manager : INotifyPropertyChanged
    {
        /// <summary>
        /// Upload thead count
        /// </summary>
        public const int MaxThreads = 4;

        private readonly ISecurityProvider _securityProvider;

        private readonly IRestApi _restApi;
        /// <summary>
        /// Replay list
        /// </summary>
        public ObservableCollectionEx<ReplayFile> Files { get; private set; } = new ObservableCollectionEx<ReplayFile>();

        private static Logger _log = LogManager.GetCurrentClassLogger();
        private bool _initialized = false;
        private List<ReplayFile> processingQueue = new List<ReplayFile>(new ConcurrentStack<ReplayFile>());
        private readonly IReplayStorage _storage;
        private HotsBpHelper.Uploader.Uploader _uploader;
        private Analyzer _analyzer;
        private Monitor _monitor;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _status = "";
        /// <summary>
        /// Current uploader status
        /// </summary>
        public string Status
        {
            get {
                return _status;
            }
        }

        private Dictionary<UploadStatus, int> _aggregates = new Dictionary<UploadStatus, int>();
        /// <summary>
        /// List of aggregate upload stats
        /// </summary>
        public Dictionary<UploadStatus, int> Aggregates
        {
            get {
                return _aggregates;
            }
        }

        /// <summary>
        /// Whether to mark replays for upload to hotslogs
        /// </summary>
        public bool UploadToHotslogs
        {
            get {
                return _uploader?.UploadToHotslogs ?? false;
            }
            set {
                if (_uploader != null) {
                    _uploader.UploadToHotslogs = value;
                }
            }
        }

        /// <summary>
        /// Which replays to delete after upload
        /// </summary>
        public DeleteFiles DeleteAfterUpload { get; set; }

        public Manager(IReplayStorage storage, ISecurityProvider securityProvider, IRestApi restApi)
        {
            this._storage = storage;
            _restApi = restApi;
            _securityProvider = securityProvider;
            Files.ItemPropertyChanged += (_, __) => { RefreshStatusAndAggregates(); };
            Files.CollectionChanged += (_, __) => { RefreshStatusAndAggregates(); };
        }

        /// <summary>
        /// Start uploading and watching for new replays
        /// </summary>
        public async void Start()
        {
            if (_initialized) {
                return;
            }
            _initialized = true;

            _uploader = new HotsBpHelper.Uploader.Uploader(_securityProvider, _restApi);
            _analyzer = new Analyzer();
            _monitor = new Monitor();

            var replays = ScanReplays();
            Files.AddRange(replays);
            replays.Where(x => x.UploadStatus == UploadStatus.None).Reverse().Map(x => processingQueue.Add(x));

            _monitor.ReplayAdded += async (_, e) => {
                await EnsureFileAvailable(e.Data, 3000);
                var replay = new ReplayFile(e.Data);
                Files.Insert(0, replay);
                processingQueue.Add(replay);
            };
            _monitor.Start();

            _analyzer.MinimumBuild = await _uploader.GetMinimumBuild();
            Task.Run(UploadLoop).Forget();
            //for (int i = 0; i < MaxThreads; i++) {
            //    Task.Run(UploadLoop).Forget();
            //}
        }
     
        private async Task UploadLoop()
        {
            while (true) {
                try
                {
                    List<ReplayFile> files = new List<ReplayFile>();

                    for (int i = 0; i < 10 && processingQueue.Any();)
                    {
                        ReplayFile file = processingQueue[0];

                        file.UploadStatus = UploadStatus.InProgress;

                        _analyzer.Analyze(file);
                        if (file.UploadStatus == UploadStatus.InProgress)
                        {
                            files.Add(file);
                            ++i;
                        }
                        processingQueue.RemoveAt(0);
                    }
                    
                    await _uploader.CheckDuplicate(files);
                    
                    foreach (var file in files)
                    {
                        var replay = _analyzer.Analyze(file);
                        // test if replay is eligible for upload (not AI, PTR, Custom, etc)
                        if (file.UploadStatus == UploadStatus.InProgress)
                        {
                            // if it is, upload it
                            await _uploader.Upload(file);
                        }
                        SaveReplayList();
                        if (ShouldDelete(file, replay))
                        {
                            DeleteReplay(file);
                        }
                    }
                }
                catch (Exception ex) {
                    _log.Error(ex, "Error in upload loop");
                }
            }
        }

        private void RefreshStatusAndAggregates()
        {
            _status = Files.Any(x => x.UploadStatus == UploadStatus.InProgress) ? "Uploading..." : "Idle";
            _aggregates = Files.GroupBy(x => x.UploadStatus).ToDictionary(x => x.Key, x => x.Count());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Aggregates)));
        }

        private void SaveReplayList()
        {
            try {
                // save only replays with fixed status. Will retry failed ones on next launch.
                var ignored = new[] { UploadStatus.None, UploadStatus.UploadError, UploadStatus.InProgress };
                _storage.Save(Files.Where(x => !ignored.Contains(x.UploadStatus)));
            }
            catch (Exception ex) {
                _log.Error(ex, "Error saving replay list");
            }
        }

        /// <summary>
        /// Load replay cache and merge it with folder scan results
        /// </summary>
        private List<ReplayFile> ScanReplays()
        {
            var replays = new List<ReplayFile>(_storage.Load());
            var lookup = new HashSet<ReplayFile>(replays);
            var comparer = new ReplayFile.ReplayFileComparer();
            replays.AddRange(_monitor.ScanReplays().Select(x => new ReplayFile(x)).Where(x => !lookup.Contains(x, comparer)));
            return replays.OrderByDescending(x => x.Created).ToList();
        }

        /// <summary>
        /// Delete replay file
        /// </summary>
        private static void DeleteReplay(ReplayFile file)
        {
            try {
                _log.Info($"Deleting replay {file}");
                file.Deleted = true;
                File.Delete(file.Filename);
            }
            catch (Exception ex) {
                _log.Error(ex, "Error deleting file");
            }
        }

        /// <summary>
        /// Ensure that HotS client finished writing replay file and it can be safely open
        /// </summary>
        /// <param name="filename">Filename to test</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <param name="testWrite">Whether to test read or write access</param>
        public async Task EnsureFileAvailable(string filename, int timeout, bool testWrite = true)
        {
            var timer = new Stopwatch();
            timer.Start();
            while (timer.ElapsedMilliseconds < timeout) {
                try {
                    if (testWrite) {
                        File.OpenWrite(filename).Close();
                    } else {
                        File.OpenRead(filename).Close();
                    }
                    return;
                }
                catch (IOException) {
                    // File is still in use
                    await Task.Delay(100);
                }
                catch {
                    return;
                }
            }
        }

        /// <summary>
        /// Decide whether a replay should be deleted according to current settings
        /// </summary>
        /// <param name="file">replay file metadata</param>
        /// <param name="replay">Parsed replay</param>
        private bool ShouldDelete(ReplayFile file, Replay replay)
        {
            return
                DeleteAfterUpload.HasFlag(DeleteFiles.PTR) && file.UploadStatus == UploadStatus.PtrRegion ||
                DeleteAfterUpload.HasFlag(DeleteFiles.Ai) && file.UploadStatus == UploadStatus.AiDetected ||
                DeleteAfterUpload.HasFlag(DeleteFiles.Custom) && file.UploadStatus == UploadStatus.CustomGame ||
                file.UploadStatus == UploadStatus.Success && (
                    DeleteAfterUpload.HasFlag(DeleteFiles.Brawl) && replay.GameMode == GameMode.Brawl ||
                    DeleteAfterUpload.HasFlag(DeleteFiles.QuickMatch) && replay.GameMode == GameMode.QuickMatch ||
                    DeleteAfterUpload.HasFlag(DeleteFiles.UnrankedDraft) && replay.GameMode == GameMode.UnrankedDraft ||
                    DeleteAfterUpload.HasFlag(DeleteFiles.HeroLeague) && replay.GameMode == GameMode.HeroLeague ||
                    DeleteAfterUpload.HasFlag(DeleteFiles.TeamLeague) && replay.GameMode == GameMode.TeamLeague
                );
        }
    }
}