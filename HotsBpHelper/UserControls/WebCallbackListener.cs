﻿using System;
using Chromium.Remote.Event;
using Chromium.WebBrowser;
using HotsBpHelper.Configuration;
using HotsBpHelper.Settings;
using Newtonsoft.Json;

namespace HotsBpHelper.UserControls
{
    public class WebCallbackListener : JSObject
    {
        public WebCallbackListener()
        {
            AddFunction("callback").Execute += Callback;
        }

        public static event EventHandler<string> InfoRequested;

        public static event EventHandler ConfigurationSaved;

        public static event EventHandler LobbyRequested;

        public static event EventHandler PresetRequested;

        public static event EventHandler StartServiceRequested;

        public static event EventHandler StopServiceRequested;

        private void Callback(object sender, CfrV8HandlerExecuteEventArgs e)
        {
            var args = e.Arguments;
            if (args.Length < 2 || !args[0].IsString)
                return;

            if (args[0].StringValue == "SaveConfig")
            {
                var newConfig = JsonConvert.DeserializeObject<CustomConfigurationSettings>(args[1].StringValue);
                BpHelperConfigParser.WriteConfig(newConfig);
                App.CustomConfigurationSettings.MMRAutoCloseTime = newConfig.MMRAutoCloseTime;
                App.CustomConfigurationSettings.UploadStrategy = newConfig.UploadStrategy;
                App.CustomConfigurationSettings.AutoUploadReplayToHotslogs = newConfig.AutoUploadReplayToHotslogs;
                App.NextConfigurationSettings = newConfig;
                OnConfigurationSaved();
            }
            if (args[0].StringValue == "SetTab")
            {
                var newTab = args[1].StringValue;
                OnInfoRequested(newTab);
            }
            if (args[0].StringValue == "RequestLobby")
            {
                OnLobbyRequested();
            }
            if (args[0].StringValue == "RequestPreset")
            {
                OnPresetRequested();
            }
            if (args[0].StringValue == "SetService")
            {
                var param = args[1].StringValue;
                if (param.Trim() == "1" || param.Trim().ToLower() == "true")
                    OnStartServiceRequested();
                
                if (param.Trim() == "0" || param.Trim().ToLower() == "false")
                    OnStopServiceRequested();
            }
        }

        protected virtual void OnInfoRequested(string e)
        {
            InfoRequested?.Invoke(this, e);
        }

        protected virtual void OnConfigurationSaved()
        {
            ConfigurationSaved?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnLobbyRequested()
        {
            LobbyRequested?.Invoke(this, EventArgs.Empty);
        }

        private static void OnPresetRequested()
        {
            PresetRequested?.Invoke(null, EventArgs.Empty);
        }

        private static void OnStartServiceRequested()
        {
            StartServiceRequested?.Invoke(null, EventArgs.Empty);
        }

        private static void OnStopServiceRequested()
        {
            StopServiceRequested?.Invoke(null, EventArgs.Empty);
        }
    }
}
