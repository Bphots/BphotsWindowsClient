using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using Chromium;
using Chromium.Event;
using Chromium.WebBrowser;
using Chromium.WebBrowser.Event;

namespace HotsBpHelper.UserControls
{
    public class ExtendedChromiumBrowser : ChromiumWebBrowser
    {
        public ExtendedChromiumBrowser()
        {
            
        }
    }

    public static class CefInitializer
    {
        public static bool IsInitialized = false;

        public static void InitializeCef()
        {
            if (IsInitialized)
                return;

            if (CfxRuntime.PlatformArch == CfxPlatformArch.x64)
                CfxRuntime.LibCefDirPath = @".\cef\Release64";
            else
                CfxRuntime.LibCefDirPath = @".\cef\Release";

            CfxRuntime.LibCfxDirPath = @".\cef\cfx";

            Chromium.WebBrowser.ChromiumWebBrowser.OnBeforeCfxInitialize += ChromiumWebBrowser_OnBeforeCfxInitialize;
            ChromiumWebBrowser.OnBeforeCommandLineProcessing += ChromiumWebBrowser_OnBeforeCommandLineProcessing;

            try
            {
                ChromiumWebBrowser.Initialize();
            }
            catch (Exception)
            {
                
                throw;
            }
            IsInitialized = true;
        }

        private static void ChromiumWebBrowser_OnBeforeCommandLineProcessing(CfxOnBeforeCommandLineProcessingEventArgs e)
        {
            Console.WriteLine("ChromiumWebBrowser_OnBeforeCommandLineProcessing");
            Console.WriteLine(e.CommandLine.CommandLineString);
        }

        private static void ChromiumWebBrowser_OnBeforeCfxInitialize(OnBeforeCfxInitializeEventArgs e)
        {
            e.Settings.LocalesDirPath = System.IO.Path.GetFullPath(@".\cef\Resources\locales");
            e.Settings.ResourcesDirPath = System.IO.Path.GetFullPath(@".\cef\Resources");
            e.Settings.BrowserSubprocessPath = System.IO.Path.GetFullPath(@".\cef\Cfx\BrowserSubProcess.exe");
            e.Settings.WindowlessRenderingEnabled = true;
            CfxRuntime.EnableHighDpiSupport();
        }
    }
}
