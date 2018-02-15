using System;
using System.Globalization;
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
            var args = Environment.GetCommandLineArgs();

            var configurationSettings = new CustomConfigurationSettings();
            BpHelperConfigParser.PopulateConfigurationSettings(configurationSettings);
            App.CustomConfigurationSettings = configurationSettings;
            App.NextConfigurationSettings = configurationSettings;

            if (args.Any(arg => arg.ToLower() == "/log"))
            {
                LogUtil.NoLog = false;
                OcrEngine.Debug = true;
            }

            if (args.Any(arg => arg.ToLower() == "/devtool"))
            {
                App.DevTool = true;
            }

            if (args.Any(arg => arg.ToLower() == "/debug"))
            {
                LogUtil.NoLog = false;
                OcrEngine.Debug = true;
                App.Debug = true;
            }

            if (args.Any(arg => arg.ToLower() == "/notcheckprocess"))
            {
                App.NotCheckProcess = true;
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
