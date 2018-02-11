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
                ErrorView _errorView = new ErrorView(ViewModelBase.L("NoMatchResolution"), ViewModelBase.L("MSG_NoMatchResolution"), "https://www.bphots.com/articles/errors/test");
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
            else
            {
                LocalizeDictionary.Instance.Culture = System.Globalization.CultureInfo.InstalledUICulture;
                var config = new HotsVariableConfigParser();
                var locale = config.CheckTextLocale();
                if (!string.IsNullOrEmpty(locale))
                {
                    switch (locale)
                    {
                        case "zhCN":
                            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
                            break;
                        case "koKR":
                            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("ko-KR");
                            break;
                        case "zhTW":
                            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("zh-TW");
                            break;
                        case "enUS":
                            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
                            break;
                        default:
                            App.OcrLanguage = OcrLanguage.Unavailable;
                            break;
                    }
                }
            }

            switch (LocalizeDictionary.Instance.Culture.Name)
            {
                case "zh-CN":
                    App.Language = "zh-CN";
                    break;
                case "ko-KR":
                    App.Language = "ko-KR";
                    break;
                case "zh-TW":
                    App.Language = "zh-TW";
                    break;
                case "zh-CHS":
                    App.Language = "zh-CN";
                    LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("zh-CN");
                    break;
                case "zh-HK":
                case "zh-CHT":
                    App.Language = "zh-TW";
                    LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("zh-TW");
                    break;
                default:
                    App.Language = "en-US";
                    LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
                    break;
            }

            if (App.OcrLanguage == OcrLanguage.Unavailable)
                return;

            if (App.Language.Contains(@"US"))
                App.OcrLanguage = OcrLanguage.English;
            if (App.Language.Contains(@"CN"))
                App.OcrLanguage = OcrLanguage.SimplifiedChinese;
            if (App.Language.Contains(@"TW"))
                App.OcrLanguage = OcrLanguage.TraditionalChinese;
            if (App.Language.Contains(@"KR"))
                App.OcrLanguage = OcrLanguage.Unavailable;
        }
    }
}
