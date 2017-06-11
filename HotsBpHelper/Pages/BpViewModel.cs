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
        public Uri LocalFileUri { get; set; }

        public BpViewModel()
        {
            string filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "index.html");
            LocalFileUri = new Uri(filePath, UriKind.Absolute);

        }

        public  void ShowHeroSelector()
        {
            var vm = new HeroSelectorViewModel(new Point(100, 200));
            WindowManager.ShowWindow(vm);
        }
    }
}