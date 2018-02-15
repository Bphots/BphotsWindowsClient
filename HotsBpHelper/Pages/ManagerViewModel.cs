﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        About
    }

    public class ManagerViewModel : ViewModelBase
    {
        public SettingsTab SettingsTab { get; set; }

        private readonly IEventAggregator _eventAggregator;

        public HashSet<SettingsTab> PopulatedTabs = new HashSet<SettingsTab>();
        
        public ManagerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            var filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "manager.html#") + App.Language;
            LocalFileUri = filePath;
        }
        
        public void ShowSettings(bool invokeWeb = true)
        {
            SettingsTab = SettingsTab.Configure;
            if (invokeWeb)
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setTab",
                Args = new[] { "Configure" }
            }, "ManagerChannel");

            if (!PopulatedTabs.Contains(SettingsTab.Configure))
            {
                PopulateSettings();
            }
        }

        public void PopulateSettings()
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "configure",
                Args = new[] {JsonConvert.SerializeObject(App.CustomConfigurationSettings)}
            }, "ManagerChannel");
            PopulatedTabs.Add(SettingsTab.Configure);
        }

        public void ShowReplays(Manager uploadManager, bool invokeWeb = true)
        {
            SettingsTab = SettingsTab.Replay;
            if (invokeWeb)
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setTab",
                Args = new[] { "Replays" }
            }, "ManagerChannel");

            if (!PopulatedTabs.Contains(SettingsTab.Replay))
            {
                PopulateUploadManager(uploadManager);
            }
        }
        public void ShowAbout(bool invokeWeb = true)
        {
            SettingsTab = SettingsTab.About;
            if (invokeWeb)
                _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
                {
                    ScriptName = "setTab",
                    Args = new[] { "About" }
                }, "ManagerChannel");

            if (!PopulatedTabs.Contains(SettingsTab.About))
            {
                PopulateAbout();
            }
        }
        public void PopulateAbout()
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "populateAbout",
                Args = new[] { JsonConvert.SerializeObject(App.About) }
            }, "ManagerChannel");
            PopulatedTabs.Add(SettingsTab.About);
        }

        private void PopulateUploadManager(Manager uploadManager)
        {
            uploadManager.ReplayFileStatusChanged -= UpdateReplay;
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "populateReplayFiles",
                Args = new[] {JsonConvert.SerializeObject(uploadManager.Files.ToList())}
            }, "ManagerChannel");
            uploadManager.ReplayFileStatusChanged += UpdateReplay;
            PopulatedTabs.Add(SettingsTab.Replay);
        }

        private void UpdateReplay(object sender, EventArgs<ReplayFile> e)
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "updateReplayFile",
                Args = new[] { JsonConvert.SerializeObject(e.Data) }
            }, "ManagerChannel");
        }

        public void ResetEvenHandler(Manager uploadManager)
        {
            uploadManager.ReplayFileStatusChanged -= UpdateReplay;
        }
        
        public string LocalFileUri { get; set; }
    }
}
