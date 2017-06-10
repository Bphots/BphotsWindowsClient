using System.Net.Mime;
using System.Windows;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly WebFileUpdaterViewModel _webFileUpdaterViewModel;

        private readonly BpViewModel _bpViewModel;

        public ShellViewModel(WebFileUpdaterViewModel webFileUpdaterViewModel, BpViewModel bpViewModel)
        {
            _webFileUpdaterViewModel = webFileUpdaterViewModel;
            _bpViewModel = bpViewModel;
        }

        protected override void OnViewLoaded()
        {
            if (WindowManager.ShowDialog(_webFileUpdaterViewModel) != true)
            {
                Application.Current.Shutdown();
            }
            WindowManager.ShowDialog(_bpViewModel);
            base.OnViewLoaded();
        }

    }
}
