using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using BpHelperMonitor.Toolkit;

namespace BpHelperMonitor
{
    public class LoaderService : ServiceBase
    {
        // Token: 0x0600000D RID: 13 RVA: 0x00002320 File Offset: 0x00000520
        private async Task Monitor()
        {
            bool hasProcess = false;
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo parentDir = Directory.GetParent(dir).Parent;
            while (true)
            {
                await Task.Delay(1000);
                var processes = Process.GetProcessesByName("HeroesOfTheStorm").Union(Process.GetProcessesByName("HeroesOfTheStorm" + "_x64")); 
                if (processes.Any() && !hasProcess)
                {
                    var hotsBpProcesses = Process.GetProcessesByName("HotsBpHelper");
                    if (!hotsBpProcesses.Any())
                    {
                        string applicationName = parentDir.FullName + @"\HotsBpHelper.exe";
                        ApplicationLoader.StartProcessAsCurrentUser(applicationName, @"/debug");
                    }
                }

                hasProcess = processes.Any();
            }
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002383 File Offset: 0x00000583
        protected override void OnStart(string[] args)
        {
            Task.Run(this.Monitor);
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002398 File Offset: 0x00000598
        protected override void OnStop()
        {
        }
    }
}
