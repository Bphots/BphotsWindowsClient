using System;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly IWindowManager _windowManager;

        private readonly WebFileUpdaterViewModel _webFileUpdaterViewModel;

        public ShellViewModel(IWindowManager windowManager, WebFileUpdaterViewModel webFileUpdaterViewModel)
        {
            _windowManager = windowManager;
            _webFileUpdaterViewModel = webFileUpdaterViewModel;
        }

        protected override void OnViewLoaded()
        {
            _windowManager.ShowDialog(_webFileUpdaterViewModel);
            base.OnViewLoaded();
        }
    }
}
