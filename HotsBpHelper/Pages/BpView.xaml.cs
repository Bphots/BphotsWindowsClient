using System;
using System.Windows;
using System.Windows.Input;
using HotsBpHelper.Messages;
using HotsBpHelper.UserControls;
using Stylet;

namespace HotsBpHelper.Pages
{
    public partial class BpView : Window, IHandle<InvokeScriptMessage>, IHandle<ShowWindowMessage>
    {
        public BpView(IEventAggregator eventAggregator)
        {
//            InitializeComponent();
            KeyDown += OnKeyDown;
            eventAggregator.Subscribe(this);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
        }

        public void Handle(InvokeScriptMessage message)
        {
            Browser.InvokeScript(message);
        }

        public void Handle(ShowWindowMessage message)
        {
            ((Window) message.ViewModel.View).Owner = this;
        }
    }
}