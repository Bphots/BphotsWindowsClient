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

namespace HotsBpHelper.Pages
{
    /// <summary>
    /// BroadcastWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BroadcastWindow : Window
    {
        public BroadcastWindow()
        {
            InitializeComponent();
        }
        public BroadcastWindow(string msg,string url)
        {
            InitializeComponent();
            broadcastInfoMsg.Text = msg;
            if (url!=null)
            {
                UrlText.Text = url;
                Uri u = new Uri(url);
                hyperlink1.NavigateUri = u;
            }
            
        }
        public void hyperlink0_Click(object s, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(hyperlink0.NavigateUri.ToString());
        }
        public void hyperlink1_Click(object s, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(hyperlink1.NavigateUri.ToString());
        }
    }
}
