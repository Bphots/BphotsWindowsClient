using System;
using System.Globalization;
using System.Linq;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Security;
using Stylet;
using StyletIoC;
using HotsBpHelper.Pages;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using WPFLocalizeExtension.Engine;

namespace HotsBpHelper
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
//            builder.Bind<IRestApi>().To<DummyRestApi>();
            builder.Bind<IRestApi>().To<RestApi>().InSingletonScope();
            builder.Bind<HeroItemUtil>().ToSelf().InSingletonScope();
            builder.Bind<MapItemUtil>().ToSelf().InSingletonScope();
            builder.Bind<ISecurityProvider>().To<SecurityProvider>().InSingletonScope();
            builder.Bind<IHeroSelectorViewModelFactory>().ToAbstractFactory();
            builder.Bind<IMapSelectorViewModelFactory>().ToAbstractFactory();
            builder.Bind<ShellViewModel.IWebFileUpdaterViewModelFactory>().ToAbstractFactory();
            builder.Bind<ShellViewModel.IBpViewModelFactory>().ToAbstractFactory();
        }

        protected override void Configure()
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
            App.AppPath = AppDomain.CurrentDomain.BaseDirectory;

            var args = Environment.GetCommandLineArgs();
            if (args.Any(arg => arg.ToLower() == "/debug"))
            {
                App.Debug = true;
            }
        }
    }
}
