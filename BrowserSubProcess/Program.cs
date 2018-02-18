using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chromium;
using Chromium.Event;

namespace BrowserSubProcess
{
    class Program
    {
        public static void Main()
        {
            if (CfxRuntime.PlatformArch == CfxPlatformArch.x64)
                CfxRuntime.LibCefDirPath = @"cef\Release64";
            else
                CfxRuntime.LibCefDirPath = @"cef\Release";
            
            CfxRuntime.LibCfxDirPath = @"cef\Cfx";
            var app = new CfxApp();
            app.OnBeforeCommandLineProcessing += AppOnOnBeforeCommandLineProcessing;
            int retval = CfxRuntime.ExecuteProcess(app);

            Environment.Exit(retval);
        }

        private static void AppOnOnBeforeCommandLineProcessing(object sender, CfxOnBeforeCommandLineProcessingEventArgs cfxOnBeforeCommandLineProcessingEventArgs)
        {
            cfxOnBeforeCommandLineProcessingEventArgs.CommandLine.AppendSwitchWithValue("disable-gpu", "1");
            cfxOnBeforeCommandLineProcessingEventArgs.CommandLine.AppendSwitchWithValue("disable-gpu-compositing", "1");
        }
    }
}
