using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using HotsBpHelper.Settings;
using NLog;

namespace HotsBpHelper.Uploader
{
    public class Monitor
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        protected FileSystemWatcher _watcher;

        /// <summary>
        /// Fires when a new replay file is found
        /// </summary>
        public event EventHandler<EventArgs<string>> ReplayAdded;

        protected virtual void OnReplayAdded(string path)
        {
            _log.Debug($"Detected new replay: {path}");
            ReplayAdded?.Invoke(this, new EventArgs<string>(path));
        }

        private bool _started = false;

        /// <summary>
        /// Starts watching filesystem for new replays. When found raises <see cref="ReplayAdded"/> event.
        /// </summary>
        public void Start()
        {
            if (_started)
                return;

            if (!Directory.Exists(Const.ProfilePath))
                return;

            _started = true;
            if (_watcher == null) {
                _watcher = new FileSystemWatcher() {
                    Path = Const.ProfilePath,
                    Filter = "*.StormReplay",
                    IncludeSubdirectories = true
                };
                _watcher.Created += (o, e) => OnReplayAdded(e.FullPath);
            }
            _watcher.EnableRaisingEvents = true;
            if (App.Debug)
                _log.Debug($"Started watching for new replays");
        }

        /// <summary>
        /// Stops watching filesystem for new replays
        /// </summary>
        public void Stop()
        {
            if (_watcher != null) {
                _watcher.EnableRaisingEvents = false;
            }
            if (App.Debug)
                _log.Debug($"Stopped watching for new replays");
        }

        /// <summary>
        /// Finds all available replay files
        /// </summary>
        public IEnumerable<string> ScanReplays()
        {
            if (!Directory.Exists(Const.ProfilePath))
                return new List<string>();

            return Directory.GetFiles(Const.ProfilePath, "*.StormReplay", SearchOption.AllDirectories).Where(l => l.Length < 240);
        }
    }
}
