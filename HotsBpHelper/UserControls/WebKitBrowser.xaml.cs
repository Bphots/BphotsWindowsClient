using System;
using System.Windows;
using System.Windows.Controls;

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
        }

        public object InvokeScript(InvokeScriptMessage message)
        {
            return Browser.InvokeScript(message.ScriptName, message.Args);
        }

        #region Source 依赖属性

        /// <summary>
        /// 依赖属性 Source 的属性名
        /// </summary>
        public const string SourcePropertyName = "Source";

        /// <summary>
        /// 取得或设置 <see cref="Source" /> 的值。这是一个依赖属性。
        /// </summary>
        public Uri Source
        {
            get { return (Uri) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// 标识 <see cref="Source" /> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            SourcePropertyName,
            typeof (Uri),
            typeof (WebKitBrowser),
            new UIPropertyMetadata(null, SourceChangedCallBack));

        private static void SourceChangedCallBack(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (WebKitBrowser) dependencyObject;
            ctrl.Browser.Navigate((Uri)e.NewValue);
        }

        #endregion
    }

    public class InvokeScriptMessage
    {
        public string ScriptName { get; set; }

        public object[] Args { get; set; }
    }
}
