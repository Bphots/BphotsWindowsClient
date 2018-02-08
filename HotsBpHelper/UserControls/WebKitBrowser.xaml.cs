using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Chromium;
using Chromium.Event;
using Chromium.Remote.Event;
using Chromium.WebBrowser;
using Chromium.WebBrowser.Event;

using HotsBpHelper.Configuration;
using HotsBpHelper.Settings;

using Stylet;
using Orientation = System.Windows.Forms.Orientation;
using Point = Hardcodet.Wpf.TaskbarNotification.Interop.Point;
using UserControl = System.Windows.Controls.UserControl;
using WebBrowser = System.Windows.Controls.WebBrowser;
using WindowStyle = System.Windows.WindowStyle;

namespace HotsBpHelper.UserControls
{
    /// <summary>
    /// Interaction logic for WebKitBrowser.xaml
    /// </summary>
    public partial class WebKitBrowser : UserControl
    {
        public WebKitBrowser()
        {
            InitializeComponent();
            Browser.BrowserCreated += BrowserOnBrowserCreated;
            Browser.LifeSpanHandler.OnBeforePopup += LifeSpanHandlerOnOnBeforePopup;
            Browser.LoadHandler.OnLoadEnd += OnLoadEnd;
            Browser.GlobalObject.AddFunction("HideWindow").Execute += HideWindow;
            //Browser.ObjectForScripting = new ScriptingHelper(this);
        }

        private void OnLoadEnd(object sender, CfxOnLoadEndEventArgs e)
        {
            var url = Browser.Url;
            if (url.AbsoluteUri.Contains("about:blank"))
            {
                Execute.OnUIThread(() =>
                {
                    Browser.LoadUrl(PendingSource);
                });
            }
        }

        private void OnRedirect(object sender, CfxOnResourceRedirectEventArgs e)
        {
            if (e.NewUrl.Contains("about:blank"))
                e.NewUrl = PendingSource;

        }
        
        private void LifeSpanHandlerOnOnBeforePopup(object sender, CfxOnBeforePopupEventArgs e)
        {
            var url = e.TargetUrl;
            Process.Start(url);
            e.SetReturnValue(true);
        }

        private void HideWindow(object sender, CfrV8HandlerExecuteEventArgs e)
        {
            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.Visibility = Visibility.Hidden;
            }
        }

        private bool _isInitialized = false;

        private void BrowserOnBrowserCreated(object sender, BrowserCreatedEventArgs browserCreatedEventArgs)
        {
            if (_isInitialized)
                return;

            if (App.DevTool)
                ShowDevTools();
            
            Execute.OnUIThread(() =>
            {
                Browser.LoadUrl(PendingSource);
            });
            _isInitialized = true;
        }


        private void ShowDevTools()
        {
            CfxWindowInfo windowInfo = new CfxWindowInfo();

            windowInfo.Style = Chromium.WindowStyle.WS_OVERLAPPEDWINDOW | Chromium.WindowStyle.WS_CLIPCHILDREN | Chromium.WindowStyle.WS_CLIPSIBLINGS | Chromium.WindowStyle.WS_VISIBLE;
            windowInfo.ParentWindow = IntPtr.Zero;
            windowInfo.WindowName = "Dev Tools";
            windowInfo.X = 200;
            windowInfo.Y = 200;
            windowInfo.Width = 800;
            windowInfo.Height = 600;
            
            Browser.BrowserHost.ShowDevTools(windowInfo, new CfxClient(), new CfxBrowserSettings(), null);
        }

        public object InvokeScript(InvokeScriptMessage message)
        {
            Execute.OnUIThread(() => Browser.ExecuteJavascript(message.ToScript()));
            
            return null;
        }

        #region Source 依赖属性

        /// <summary>
        /// 依赖属性 Source 的属性名
        /// </summary>
        public const string SourcePropertyName = "Source";

        public string PendingSource { get; set; }

        /// <summary>
        /// 取得或设置 <see cref="Source" /> 的值。这是一个依赖属性。
        /// </summary>
        public string Source
        {
            get { return (string) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="Source" /> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            SourcePropertyName,
            typeof (string),
            typeof (WebKitBrowser),
            new UIPropertyMetadata(null, SourceChangedCallBack));

        private static void SourceChangedCallBack(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (WebKitBrowser) dependencyObject;
            if (ctrl.Browser.Created && ctrl.Browser != null)
                Execute.OnUIThread(() => ctrl.Browser.LoadUrl((string)e.NewValue));

            ctrl.PendingSource = (string)e.NewValue;
        }


        #endregion


        /// <summary>
        /// 取得或设置 <see cref="Source" /> 的值。这是一个依赖属性。
        /// </summary>
        public bool ShowDevTool
        {
            get { return (bool)GetValue(ShowDevToolProperty); }
            set { SetValue(ShowDevToolProperty, value); }
        }

        /// <summary>
        /// 依赖属性 Source 的属性名
        /// </summary>
        public const string ShowDevToolPropertyName = "ShowDevTool";

        /// <summary>
        /// 标识 <see cref="ShowDevTool" /> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty ShowDevToolProperty = DependencyProperty.Register(
            ShowDevToolPropertyName,
            typeof(bool),
            typeof(WebKitBrowser),
            new UIPropertyMetadata(false, ShowDevToolChangedCallBack));

        private static void ShowDevToolChangedCallBack(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (WebKitBrowser)dependencyObject;
            bool show = (bool)e.NewValue;
            if (show)
                Execute.OnUIThread(() => ctrl.ShowDevTools());
            else
                Execute.OnUIThread(() => ctrl.CloseDevTools());
        }

        private void CloseDevTools()
        {
            Browser.BrowserHost.CloseDevTools();
        }
    }

    public class InvokeScriptMessage
    {
        public string ScriptName { get; set; }

        public string[] Args { get; set; }

        public string ToScript() => ScriptName + "(" + string.Join(",", Args.Select(m => "'" + m + "'")) + ");";
    }
}
