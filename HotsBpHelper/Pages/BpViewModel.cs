using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using HotsBpHelper.Utils;

namespace HotsBpHelper.Pages
{
    public class BpViewModel : ViewModelBase
    {
        private readonly IHeroSelectorViewModelFactory _heroSelectorViewModelFactory;

        public Uri LocalFileUri { get; set; }

        public BpViewModel(IHeroSelectorViewModelFactory heroSelectorViewModelFactory)
        {
            _heroSelectorViewModelFactory = heroSelectorViewModelFactory;
            string filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "index.html");
            LocalFileUri = new Uri(filePath, UriKind.Absolute);

        }

        public  void ShowHeroSelector()
        {
            var vm = _heroSelectorViewModelFactory.CreateHeroSelectorViewModel();
            vm.Location = new Point(174, 115).ToPixelPoint();
            WindowManager.ShowWindow(vm);
        }
    }

    public interface IHeroSelectorViewModelFactory
    {
        HeroSelectorViewModel CreateHeroSelectorViewModel();
    }
}