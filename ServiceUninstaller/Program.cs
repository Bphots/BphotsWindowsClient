using System;
using System.Diagnostics;

namespace ServiceUninstaller
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("Uninstalling...");
                var process = Process.Start(new ProcessStartInfo()
                {
                    FileName = @"cmd.exe",
                    UseShellExecute = true,
                    Arguments = "/C sc stop \"HotsBpHelper - Monitor\"&sc delete \"HotsBpHelper - Monitor\"",
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                process.WaitForExit();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }
    }
}
