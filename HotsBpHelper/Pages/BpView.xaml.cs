using System.Windows;
using HotsBpHelper.UserControls;
using Stylet;

namespace HotsBpHelper.Pages
{
    public partial class BpView : Window, IHandle<InvokeScriptMessage>
    {
        public BpView(IEventAggregator eventAggregator)
        {
//            InitializeComponent();
            eventAggregator.Subscribe(this);
        }

        public void Handle(InvokeScriptMessage message)
        {
            Browser.InvokeScript(message);
        }
    }
}