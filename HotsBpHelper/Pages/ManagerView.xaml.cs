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

        public void RegisterTitleHandler()
        {
            Browser.Browser.DisplayHandler.OnTitleChange += (s, e2) =>
            {
                var title = e2.Title;
                Execute.OnUIThread(
                    () =>
                        Title =
                            @"HotsBpHelper - " + title);
            };
        }

        public void Handle(InvokeScriptMessage message)
        {
            Browser.InvokeScript(message);
        }

        private void ManagerView_OnClosed(object sender, EventArgs e)
        {
            Browser?.Browser?.Dispose();
        }
    }
}
