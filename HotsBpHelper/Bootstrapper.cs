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
            
            App.AppPath = AppDomain.CurrentDomain.BaseDirectory;

            var args = Environment.GetCommandLineArgs();
            if (args.Any(arg => arg.ToLower() == "/debug"))
            {
                App.Debug = true;
            }
            if (args.Any(arg => arg.ToLower() == "/errortest"))
            {
                ErrorView _errorView = new ErrorView(ViewModelBase.L("NoMatchResolution"), ViewModelBase.L("MSG_NoMatchResolution"), "http://www.bphots.com/articles/QA/test");
                _errorView.ShowDialog();
            }

            if (args.Any(arg => arg.ToLower() == "/cn"))
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");                
            }
            else if (args.Any(arg => arg.ToLower() == "/us"))
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
            }
            else if (args.Any(arg => arg.ToLower() == "/kr"))
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("ko-KR");
            }
            else if (args.Any(arg => arg.ToLower() == "/tw"))
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("zh-TW");
            }
            else if (args.Any(arg => arg.ToLower() == "/jp"))
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("ja-JP");
            }
            else LocalizeDictionary.Instance.Culture = System.Globalization.CultureInfo.InstalledUICulture;
            switch (LocalizeDictionary.Instance.Culture.Name) {
                case "zh-CN":
                    break;
                case "zh-CHS":
                    App.Language = "zh-CN";
                    break;
                case "ko-KR":
                    break;
                case "zh-TW":
                    break;
                case "zh-HK":
                    App.Language = "zh-TW";
                    break;
                case "zh-CHT":
                    App.Language = "zh-TW";
                    break;
                default:
                    App.Language = "en-US";
                    return;
            }

            App.Language = LocalizeDictionary.Instance.Culture.Name;
        }
    }
}
