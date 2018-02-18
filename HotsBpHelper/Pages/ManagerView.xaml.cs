using System;
using System.Windows;
using HotsBpHelper.UserControls;
using Stylet;

namespace HotsBpHelper.Pages
{
    /// <summary>
    ///     Interaction logic for ManagerView.xaml
    /// </summary>
    public partial class ManagerView : Window, IHandle<InvokeScriptMessage>
    {
        public ManagerView(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this, "ManagerChannel");
        }

        public void Handle(InvokeScriptMessage message)
        {
            Browser.InvokeScript(message);
        }

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

        private void ManagerView_OnClosed(object sender, EventArgs e)
        {
            Browser?.DisposeBrowser();
        }
    }
}