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


        #region Source 依赖属性

        /// <summary>
        /// 依赖属性 Source 的属性名
        /// </summary>
        public const string SourcePropertyName = "Source";

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
        }

        #endregion
    }
}
