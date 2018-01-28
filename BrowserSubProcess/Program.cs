using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chromium;

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
            int retval = CfxRuntime.ExecuteProcess();

            Environment.Exit(retval);
        }
    }
}
