using System.Windows;
using HotsBpHelper.UserControls;
using Stylet;

namespace HotsBpHelper.Pages
{
    public partial class BpView : Window, IHandle<InvokeScriptParameter>
    {
        public BpView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            eventAggregator.Subscribe(this);
        }

        public void Handle(InvokeScriptParameter message)
        {
            Browser.InvokeScript(message);
        }
    }
}