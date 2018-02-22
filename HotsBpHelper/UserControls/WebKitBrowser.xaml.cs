using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using Chromium;
using Chromium.Event;
using Chromium.Remote.Event;
using Chromium.WebBrowser;
using Chromium.WebBrowser.Event;
using Stylet;
using UserControl = System.Windows.Controls.UserControl;
using WindowStyle = Chromium.WindowStyle;

namespace HotsBpHelper.UserControls
{
    /// <summary>
    ///     Interaction logic for WebKitBrowser.xaml
    /// </summary>
    public partial class WebKitBrowser : UserControl, IDisposable
    {
        /// <summary>
        ///     依赖属性 Source 的属性名
        /// </summary>
        public const string ShowDevToolPropertyName = "ShowDevTool";

        /// <summary>
        ///     标识 <see cref="Source" /> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            SourcePropertyName,
            typeof (string),
            typeof (WebKitBrowser),
            new UIPropertyMetadata(null, SourceChangedCallBack));

        /// <summary>
        ///     标识 <see cref="ShowDevTool" /> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty ShowDevToolProperty = DependencyProperty.Register(
            ShowDevToolPropertyName,
            typeof (bool),
            typeof (WebKitBrowser),
            new UIPropertyMetadata(false, ShowDevToolChangedCallBack));

        private readonly ConcurrentQueue<string> Scripts = new ConcurrentQueue<string>();

        private bool _isInitialized;

        private bool _isLoaded;

        public WebKitBrowser()
        {
            InitializeComponent();
            Browser = new ExtendedChromiumBrowser();
            Browser.BrowserCreated += BrowserOnBrowserCreated;
            Browser.LifeSpanHandler.OnBeforePopup += LifeSpanHandlerOnOnBeforePopup;
            Browser.LoadHandler.OnLoadEnd += OnLoadEnd;
            Browser.GlobalObject.AddFunction("HideWindow").Execute += HideWindow;
            Host.Child = Browser;
        }
        

        public ExtendedChromiumBrowser Browser { get; set; }


        /// <summary>
        ///     取得或设置 <see cref="Source" /> 的值。这是一个依赖属性。
        /// </summary>
        public bool ShowDevTool
        {
            get { return (bool) GetValue(ShowDevToolProperty); }
            set { SetValue(ShowDevToolProperty, value); }
        }

        public void InitializeBrowser(string url = null)
        {
            Execute.OnUIThread(() =>
            {
                if (Browser == null || Browser.IsDisposed)
                {
                    _isLoaded = false;
                    Browser = new ExtendedChromiumBrowser(url);
                    Browser.BrowserCreated += BrowserOnBrowserCreated;
                    Browser.LifeSpanHandler.OnBeforePopup += LifeSpanHandlerOnOnBeforePopup;
                    Browser.LoadHandler.OnLoadEnd += OnLoadEnd;
                    Browser.GlobalObject.AddFunction("HideWindow").Execute += HideWindow;
                    Host.Child = Browser;
                    Host.Child.Refresh();
                }
                else if (!_isLoaded)
                {
                    PendingSource = url;
                }
                else
                {
                    Browser.LoadUrl(url);
                }
            });
        }

        public void DisposeBrowser()
        {
            Execute.OnUIThread(() =>
            {
                if (Browser != null && !Browser.IsDisposed)
                    Browser.Dispose();
            });
        }

        private void OnLoadEnd(object sender, CfxOnLoadEndEventArgs e)
        {
            var url = Browser.Url;
            if (url.AbsoluteUri.Contains("about:blank"))
            {
                Execute.OnUIThread(() => Browser.LoadUrl(PendingSource));
            }
            if (!url.AbsoluteUri.Contains("about:blank"))
            {
                if (App.DevTool)
                    ShowDevTools();

                Execute.OnUIThread(() =>
                {
                    while (Scripts.Any())
                    {
                        string script;
                        if (Scripts.TryDequeue(out script))
                        {
                            Browser.ExecuteJavascript(script);
                        }
                    }

                    _isLoaded = true;
                }
               );
            }
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
            DisposeBrowser();
        }

        private void BrowserOnBrowserCreated(object sender, BrowserCreatedEventArgs browserCreatedEventArgs)
        {
            if (_isInitialized)
                return;

            Execute.OnUIThread(() =>
            {
                Browser.LoadUrl(PendingSource);
                _isInitialized = true;
            });
        }


        public void ShowDevTools()
        {
            var windowInfo = new CfxWindowInfo();

            windowInfo.Style = WindowStyle.WS_OVERLAPPEDWINDOW | WindowStyle.WS_CLIPCHILDREN |
                               WindowStyle.WS_CLIPSIBLINGS | WindowStyle.WS_VISIBLE;
            windowInfo.ParentWindow = IntPtr.Zero;
            windowInfo.WindowName = "Dev Tools - " + PendingSource;
            windowInfo.X = 200;
            windowInfo.Y = 200;
            windowInfo.Width = 800;
            windowInfo.Height = 600;

            Browser.BrowserHost.ShowDevTools(windowInfo, new CfxClient(), new CfxBrowserSettings(), null);
        }

        public object InvokeScript(InvokeScriptMessage message)
        {
            Execute.OnUIThread(() =>
            {
                if ((Browser == null || Browser.IsDisposed) && !string.IsNullOrEmpty(PendingSource))
                    InitializeBrowser(PendingSource);

                if (Browser.IsLoading || !_isInitialized || Scripts.Any() || !_isLoaded)
                    Scripts.Enqueue(message.ToScript());
                else
                    Browser.ExecuteJavascript(message.ToScript());
            });

            return null;
        }

        private static void ShowDevToolChangedCallBack(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (WebKitBrowser) dependencyObject;
            var show = (bool) e.NewValue;
            if (show)
                Execute.OnUIThread(() => ctrl.ShowDevTools());
            else
                Execute.OnUIThread(() => ctrl.CloseDevTools());
        }

        private void CloseDevTools()
        {
            Browser.BrowserHost.CloseDevTools();
        }

        #region Source 依赖属性

        /// <summary>
        ///     依赖属性 Source 的属性名
        /// </summary>
        public const string SourcePropertyName = "Source";

        public string PendingSource { get; set; }

        /// <summary>
        ///     取得或设置 <see cref="Source" /> 的值。这是一个依赖属性。
        /// </summary>
        public string Source
        {
            get { return (string) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }


        private static void SourceChangedCallBack(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (WebKitBrowser) dependencyObject;
            if (ctrl.Browser.Created && ctrl.Browser != null)
                Execute.OnUIThread(() => ctrl.Browser.LoadUrl((string) e.NewValue));

            ctrl.PendingSource = (string) e.NewValue;
        }

        #endregion

        public void Dispose()
        {
            DisposeBrowser();
        }
    }

    public class InvokeScriptMessage
    {
        public string ScriptName { get; set; }

        public string[] Args { get; set; }

        public string ToScript()
            =>
                ScriptName + "(" + string.Join(",", Args.Select(m => "'" + HttpUtility.JavaScriptStringEncode(m) + "'")) +
                ");";
    }
}