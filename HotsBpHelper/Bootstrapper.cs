using System;
using System.Globalization;
using HotsBpHelper.Api;
using Stylet;
using StyletIoC;
using HotsBpHelper.Pages;
using HotsBpHelper.Utils.HeroUtil;
using WPFLocalizeExtension.Engine;

namespace HotsBpHelper
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
//            builder.Bind<IRestApi>().To<DummyRestApi>();
            builder.Bind<IRestApi>().To<RestApi>().InSingletonScope();
            builder.Bind<IHeroUtil>().To<HeroUtil>().InSingletonScope();
            builder.Bind<IHeroSelectorViewModelFactory>().ToAbstractFactory();
            builder.Bind<ShellViewModel.IWebFileUpdaterViewModelFactory>().ToAbstractFactory();
            builder.Bind<ShellViewModel.IBpViewModelFactory>().ToAbstractFactory();
        }

        protected override void Configure()
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentUICulture;
            App.AppPath = AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
