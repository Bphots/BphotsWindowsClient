using System;
using System.ComponentModel;
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
            var win = GetWindow(this);
            if (win != null && win.Visibility == Visibility.Visible)
            {
                Browser.InvokeScript(message);
            }
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

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            var win = GetWindow(this);
            if (win != null)
            {
                win.Visibility = Visibility.Hidden;
            }
            Browser?.DisposeBrowser();
            OnHide();
            //Do whatever you want here..
        }

        public void ShowWindow()
        {
            var win = GetWindow(this);
            if (win != null)
            {
                win.Visibility = Visibility.Visible;
            }
        }

        public event EventHandler HideRequested;

        protected virtual void OnHide()
        {
            HideRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}