using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotsBpHelper.UserControls;
using Newtonsoft.Json;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class ManagerViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        
        public ManagerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            var filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "manager.html#") + App.Language;
            LocalFileUri = filePath;
        }

        public void ShowSettings()
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setTab",
                Args = new[] { "Settings" }
            }, "ManagerChannel");

            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "configure",
                Args = new[] { JsonConvert.SerializeObject(App.CustomConfigurationSettings) }
            }, "ManagerChannel");
        }
        
        public string LocalFileUri { get; set; }
    }
}
