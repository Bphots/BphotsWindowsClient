using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Chromium.WebBrowser;
using HotsBpHelper.UserControls;
using Stylet;

namespace HotsBpHelper.Pages
{
    /// <summary>
    /// Interaction logic for ManagerView.xaml
    /// </summary>
    public partial class ManagerView : Window, IHandle<InvokeScriptMessage>
    {
        public ManagerView(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this, "ManagerChannel");
        }

        public event EventHandler<SettingsTab> TabInfoRequested;

        public event EventHandler ConfigurationSaved;

        public void RegisterTitleHandler()
        {
            Browser.Browser.DisplayHandler.OnTitleChange += (s, e2) =>
            {
                var title = e2.Title;
                Execute.OnUIThread(
                    () =>
                        Title =
                            ViewModelBase.L("HotsBpHelper") + " - " + title);
            };
        }

        public void RegisterCallbackObject()
        {
            var callbackObject = new WebCallbackListener();
            callbackObject.InfoRequested += CallbackObjectOnInfoRequested;
            callbackObject.ConfigurationSaved += OnConfigurationSaved;
            Browser.Browser.GlobalObject.Add("CallbackObject", callbackObject);
        }

        private void OnConfigurationSaved(object sender, EventArgs e)
        {
            ConfigurationSaved?.Invoke(this, EventArgs.Empty);
        }

        private void CallbackObjectOnInfoRequested(object sender, string s)
        {
            if (s == "Configure")
                OnTabInfoRequested(SettingsTab.Configure);
            if (s == "Replays")
                OnTabInfoRequested(SettingsTab.Replay);
            if (s == "About")
                OnTabInfoRequested(SettingsTab.About);
        }

        public void Handle(InvokeScriptMessage message)
        {
            Browser.InvokeScript(message);
        }

        private void ManagerView_OnClosed(object sender, EventArgs e)
        {
            Browser?.Browser?.Dispose();
        }

        protected virtual void OnTabInfoRequested(SettingsTab e)
        {
            TabInfoRequested?.Invoke(this, e);
        }
    }
}
