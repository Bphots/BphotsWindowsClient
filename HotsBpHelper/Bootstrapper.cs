using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Security;
using HotsBpHelper.Configuration;
using HotsBpHelper.Factories;
using Stylet;
using StyletIoC;
using HotsBpHelper.Pages;
using HotsBpHelper.Services;
using HotsBpHelper.Settings;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using ImageProcessor.Ocr;
using WPFLocalizeExtension.Engine;

namespace HotsBpHelper
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Bind<IRestApi>().To<RestApi>().InSingletonScope();
            builder.Bind<HeroItemUtil>().ToSelf().InSingletonScope();
            builder.Bind<MapItemUtil>().ToSelf().InSingletonScope();
            builder.Bind<ISecurityProvider>().To<SecurityProvider>().InSingletonScope();
            builder.Bind<IToastService>().To<ToastService>().InSingletonScope();

            RegisterViewModelFactories(builder);

            builder.Bind<IImageUtil>().To<ImageUtils>();
        }

        private static void RegisterViewModelFactories(IStyletIoCBuilder builder)
        {
            var vmTypeList = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where typeof (ViewModelBase).IsAssignableFrom(assemblyType)
                select assemblyType).Where(vmType => !vmType.IsGenericType && !vmType.IsAbstract).ToArray();

            var method = typeof (IStyletIoCBuilder).GetMethod("Bind", new Type[] {});
            foreach (var vm in vmTypeList)
            {
                var generic = method?.MakeGenericMethod(vm);
                var bindTo = generic?.Invoke(builder, null) as IBindTo;
                bindTo?.ToSelf();
            }

            builder.Bind<ViewModelFactory>().ToSelf().InSingletonScope();
        }

        protected override void Configure()
        {
            App.AppPath = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(App.AppPath);

            var args = Environment.GetCommandLineArgs();

            try
            {
                var configurationSettings = new CustomConfigurationSettings();
                BpHelperConfigParser.PopulateConfigurationSettings(configurationSettings);
                UserDataConfigParser.PopulateUserDataSettings();
                App.CustomConfigurationSettings = configurationSettings;
                App.NextConfigurationSettings = configurationSettings;
            }
            catch (Exception)
            {
                //Ignored
            }
            
            if (args.Any(arg => arg.ToLower() == "/log"))
            {
                LogUtil.NoLog = false;
                OcrEngine.Debug = true;
            }

            if (args.Any(arg => arg.ToLower() == "/devtool"))
            {
                App.DevTool = true;
            }

            if (args.Any(arg => arg.ToLower() == "/debugv2"))
            {
                LogUtil.NoLog = false;
                OcrEngine.Debug = true;
                App.Debug = true;
            }

            if (args.Any(arg => arg.ToLower() == "/debugv2"))
            {
                LogUtil.NoLog = false;
                OcrEngine.Debug = true;
                App.Debug = true;
            }

            if (args.Any(arg => arg.ToLower() == "/forceupdate"))
                App.ForceUpdate = true;

            try
            {
                if (args.Any(arg => arg.ToLower().StartsWith("/upload")))
                {
                    var dateString = args.First(arg => arg.ToLower().StartsWith("/upload")).Substring(7);
                    App.UploadMinimumAcceptableTime = DateTime.Parse(dateString).ToUniversalTime();
                }
                else
                    App.UploadMinimumAcceptableTime = Const.HotsweekAcceptTime;
            }
            catch
            {
                App.UploadMinimumAcceptableTime = DateTime.Now;
            }

            if (args.Any(arg => arg.ToLower() == "/errortest"))
            {
                ErrorView _errorView = new ErrorView(ViewModelBase.L("NoMatchResolution"),
                    ViewModelBase.L("MSG_NoMatchResolution"), "https://www.bphots.com/articles/errors/test");
                _errorView.ShowDialog();
            }
        }
    }
}
