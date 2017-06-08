using System;
using System.Globalization;
using HotsBpHelper.Api;
using Stylet;
using StyletIoC;
using HotsBpHelper.Pages;
using WPFLocalizeExtension.Engine;

namespace HotsBpHelper
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
//            builder.Bind<IRestApi>().To<DummyRestApi>();
            builder.Bind<IRestApi>().To<RestApi>();
            builder.Autobind();
        }

        protected override void Configure()
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentUICulture;
            App.AppPath = AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
