using System;
using System.Diagnostics;
using System.IO;

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
    }
}