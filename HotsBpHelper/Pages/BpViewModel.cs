using System;
using System.IO;

namespace HotsBpHelper.Pages
{
    public class BpViewModel : ViewModelBase
    {
        public Uri LocalFileUri { get; set; }

        public BpViewModel()
        {
            string filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "index.html");
//            filePath = @"c:\Users\Administrator\Desktop\HotsBpHelper\bp_helper v.0.44\index.html";
            LocalFileUri = new Uri("file:///" + filePath);
        }
    }
}