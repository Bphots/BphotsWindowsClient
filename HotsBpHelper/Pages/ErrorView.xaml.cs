using System;
using System.Diagnostics;
using System.Windows;
using Application = System.Windows.Forms.Application;

namespace HotsBpHelper.Pages
{
    /// <summary>
    ///     ErrorView.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorView : Window
    {
        public bool isPause = true, isShutDown = true;

        public ErrorView()
        {
            InitializeComponent();
        }

        public ErrorView(string errorInfo)
        {
            InitializeComponent();
            this.errorInfo.Text = errorInfo;
        }

        public ErrorView(string errorName, string errorInfo)
        {
            InitializeComponent();
            Title = errorName;
            this.errorInfo.Text = errorInfo;
        }

        public ErrorView(string errorName, string errorInfo, string url)
        {
            InitializeComponent();
            Title = errorName;
            this.errorInfo.Text = errorInfo;
            if (url != null)
            {
                var u = new Uri(url);
                hyperlink1.NavigateUri = u;
                UrlText.Text = url;
            }
        }

        public void Pause()
        {
            while (isPause)
            {
                Application.DoEvents();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (isShutDown)
            {
                Environment.Exit(0);
            }
            base.OnClosed(e);
        }

        public void hyperlink0_Click(object s, RoutedEventArgs e)
        {
            Process.Start(hyperlink0.NavigateUri.ToString());
        }

        public void hyperlink1_Click(object s, RoutedEventArgs e)
        {
            Process.Start(hyperlink1.NavigateUri.ToString());
        }
    }
}