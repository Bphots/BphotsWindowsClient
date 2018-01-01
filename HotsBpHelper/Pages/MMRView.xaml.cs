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
    /// Interaction logic for MMRView.xaml
    /// </summary>
    public partial class MMRView : Window, IHandle<InvokeScriptMessage>
    {
        public MMRView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            eventAggregator.Subscribe(this, "MMRChanel");
        }

        public void Handle(InvokeScriptMessage message)
        {
            Browser.InvokeScript(message);
        }
    }
}
