using System.Net.Mime;
using System.Windows;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly WebFileUpdaterViewModel _webFileUpdaterViewModel;

        public ShellViewModel( WebFileUpdaterViewModel webFileUpdaterViewModel)
        {
            _webFileUpdaterViewModel = webFileUpdaterViewModel;
        }

        protected override void OnViewLoaded()
        {
            if (WindowManager.ShowDialog(_webFileUpdaterViewModel) != true)
            {
                Application.Current.Shutdown();
            }
            base.OnViewLoaded();
        }

    }
}
