using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HotsBpHelper.Uploader;
using HotsBpHelper.UserControls;
using Newtonsoft.Json;
using Stylet;

namespace HotsBpHelper.Pages
{
    public enum SettingsTab
    {
        Replay,
        Configure,
        BigData,
        About,
        Hotsweek
    }

    public class ManagerViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;

        public HashSet<SettingsTab> PopulatedTabs = new HashSet<SettingsTab>();

        public ManagerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            var filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "manager.html#") + App.Language;
            LocalFileUri = filePath;
            WebCallbackListener.ConfigurationSaved += OnConfigurationSaved;
            WebCallbackListener.InfoRequested += OnInfoRequested;
        }

        public SettingsTab SettingsTab { get; set; }

        public string LocalFileUri { get; set; }

        public static event EventHandler TabChanged;

        public Manager UploadManager { get; set; }

        public bool IsClosed { get; set; }

        private void OnInfoRequested(object sender, string s)
        {
            if (s == "Configure")
                ShowSettings(false);
            if (s == "Replays")
                ShowReplays(false);
            if (s == "About")
                ShowAbout(false);
            if (s == "Hotsweek")
                ShowHotsweek(false);
        }

        private void OnConfigurationSaved(object sender, EventArgs eventArgs)
        {
            UploadManager.RepopulateQueue();
        }


        public void ShowSettings(bool invokeWeb = true)
        {
            SettingsTab = SettingsTab.Configure;
            if (invokeWeb)
                _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
                {
                    ScriptName = "setTab",
                    Args = new[] {"Configure"}
                }, "ManagerChannel");

            if (!PopulatedTabs.Contains(SettingsTab.Configure))
            {
                PopulateSettings();
            }

            OnTabChanged();
        }

        public void PopulateSettings()
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "configure",
                Args = new[] {JsonConvert.SerializeObject(App.NextConfigurationSettings)}
            }, "ManagerChannel");
            PopulatedTabs.Add(SettingsTab.Configure);
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setIsServiceRunning",
                Args = new[] { JsonConvert.SerializeObject(ShellViewModel.GetIsServiceRuning()) }
            }, "ManagerChannel");
        }

        public void ShowReplays(bool invokeWeb = true)
        {
            SettingsTab = SettingsTab.Replay;
            if (invokeWeb)
                _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
                {
                    ScriptName = "setTab",
                    Args = new[] {"Replays"}
                }, "ManagerChannel");

            if (!PopulatedTabs.Contains(SettingsTab.Replay))
            {
                PopulateUploadManager();
            }

            OnTabChanged();
        }

        public void ShowAbout(bool invokeWeb = true)
        {
            SettingsTab = SettingsTab.About;
            if (invokeWeb)
                _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
                {
                    ScriptName = "setTab",
                    Args = new[] {"About"}
                }, "ManagerChannel");

            if (!PopulatedTabs.Contains(SettingsTab.About))
            {
                PopulateAbout();
            }

            OnTabChanged();
        }

        public void ShowHotsweek(bool invokeWeb = true)
        {
            SettingsTab = SettingsTab.Hotsweek;
            if (invokeWeb)
                _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
                {
                    ScriptName = "setTab",
                    Args = new[] { "Hotsweek" }
                }, "ManagerChannel");

            if (!PopulatedTabs.Contains(SettingsTab.Hotsweek))
            {
                PopulateHotsweek();
            }

            OnTabChanged();
        }

        public void PopulateAbout()
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "populateAbout",
                Args = new[] {JsonConvert.SerializeObject(App.About)}
            }, "ManagerChannel");
            PopulatedTabs.Add(SettingsTab.About);
        }

        public void PopulateHotsweek()
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "populateHotsweek",
                Args = new[] { App.CustomConfigurationSettings.HotsweekPlayerId, App.CustomConfigurationSettings.LanguageForBphots }
            }, "ManagerChannel");
            PopulatedTabs.Add(SettingsTab.Hotsweek);
        }

        private void PopulateUploadManager()
        {
            UploadManager.ReplayFileStatusChanged -= UpdateReplay;
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "populateReplayFiles",
                Args = new[] {JsonConvert.SerializeObject(UploadManager.Files.ToList())}
            }, "ManagerChannel");
            UploadManager.ReplayFileStatusChanged += UpdateReplay;
            PopulatedTabs.Add(SettingsTab.Replay);
        }

        private void UpdateReplay(object sender, EventArgs<ReplayFile> e)
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "updateReplayFile",
                Args = new[] {JsonConvert.SerializeObject(e.Data)}
            }, "ManagerChannel");
        }

        public void ResetEvenHandler()
        {
            UploadManager.ReplayFileStatusChanged -= UpdateReplay;
        }

        private static void OnTabChanged()
        {
            TabChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}