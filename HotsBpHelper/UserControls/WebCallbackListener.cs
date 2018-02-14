using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public event EventHandler<string> InfoRequested;

        private void Callback(object sender, CfrV8HandlerExecuteEventArgs e)
        {
            var args = e.Arguments;
            if (args.Length < 2 || !args[0].IsString)
                return;

            if (args[0].StringValue == "SaveConfig")
            {
                var newConfig = JsonConvert.DeserializeObject<CustomConfigurationSettings>(args[1].StringValue);
                BpHelperConfigParser.WriteConfig(newConfig);
                App.CustomConfigurationSettings = newConfig;
            }
            if (args[0].StringValue == "SetTab")
            {
                var newTab = args[1].StringValue;
                OnInfoRequested(newTab);
            }
        }

        protected virtual void OnInfoRequested(string e)
        {
            InfoRequested?.Invoke(this, e);
        }
    }
}
