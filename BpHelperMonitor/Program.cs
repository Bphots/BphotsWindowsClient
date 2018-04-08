using System.ServiceProcess;

namespace BpHelperMonitor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ServiceBase[] services =
            {
                new LoaderService()
            };
            ServiceBase.Run(services);
        }
    }
}