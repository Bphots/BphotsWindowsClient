using System;
using System.Windows;

namespace HotsBpHelper.Pages
{
    /// <summary>
    /// ErrorView.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorView : Window
    {
        public bool isPause = true,isShutDown=true;

        public void Pause()
        {
            while (isPause)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        public ErrorView()
        {
            InitializeComponent();
        }

        public ErrorView(string errorInfo)
        {
            InitializeComponent();
            this.errorInfo.Text = errorInfo;
        }

        public ErrorView(string errorName,string errorInfo)
        {

            InitializeComponent();
            this.Title = errorName;
            this.errorInfo.Text = errorInfo;
        }

        public ErrorView(string errorName, string errorInfo, string url)
        {

            InitializeComponent();
            this.Title = errorName;
            this.errorInfo.Text = errorInfo;
            if (url!=null) {
                Uri u = new Uri(url);
                hyperlink1.NavigateUri = u;
                UrlText.Text = url;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            
            if (isShutDown)
            {
                Environment.Exit(0);
                //isPause = false;
            }
            isPause = false;
            base.OnClosed(e);
        }
        public void hyperlink0_Click(object s,RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(hyperlink0.NavigateUri.ToString());  
        }
        public void hyperlink1_Click(object s, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(hyperlink1.NavigateUri.ToString());
        }
    }
}
