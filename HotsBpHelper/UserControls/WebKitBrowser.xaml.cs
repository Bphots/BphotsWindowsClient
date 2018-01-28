using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Chromium;
using Chromium.Remote.Event;
using Chromium.WebBrowser;
using Chromium.WebBrowser.Event;
using Stylet;
using Point = Hardcodet.Wpf.TaskbarNotification.Interop.Point;
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

            Browser.GlobalObject.AddFunction("HideWindow").Execute += HideWindow;
            //Browser.ObjectForScripting = new ScriptingHelper(this);
        }

        private void HideWindow(object sender, CfrV8HandlerExecuteEventArgs e)
        {
            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.Visibility = Visibility.Hidden;
            }
        }

        private void BrowserOnBrowserCreated(object sender, BrowserCreatedEventArgs browserCreatedEventArgs)
        {
            Execute.OnUIThread(() =>
            {
                var dpiPoint = GetSystemDpi();
                double zoom = 0 - (dpiPoint.X-96) / 24;
                if (Math.Abs(zoom) > 0.01)
                    Browser.BrowserHost.ZoomLevel = zoom;
            });
            Execute.OnUIThread(() =>
            {
                Browser.LoadUrl(PendingSource);
            });
        }

        #region Get Font DPI

        private static readonly int LOGPIXELSX = 88;    // Used for GetDeviceCaps().
        private static readonly int LOGPIXELSY = 90;    // Used for GetDeviceCaps().

        /// <summary>Determines the current screen resolution in DPI.</summary>
        /// <returns>Point.X is the X DPI, Point.Y is the Y DPI.</returns>
        public static Point GetSystemDpi()
        {
            Point result = new Point();

            IntPtr hDC = GetDC(IntPtr.Zero);

            result.X = GetDeviceCaps(hDC, LOGPIXELSX);
            result.Y = GetDeviceCaps(hDC, LOGPIXELSY);

            ReleaseDC(IntPtr.Zero, hDC);

            return result;
        }
        

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        #endregion

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
