using System;
using System.Linq;
using System.Net.Mime;
using System.Windows;
using HotsBpHelper.Settings;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly IWebFileUpdaterViewModelFactory _webFileUpdaterViewModelFactory;

        private readonly IBpViewModelFactory _bpViewModelFactory;


        public ShellViewModel(IWebFileUpdaterViewModelFactory webFileUpdaterViewModelFactory, IBpViewModelFactory bpViewModelFactory)
        {
            _webFileUpdaterViewModelFactory = webFileUpdaterViewModelFactory;
            _bpViewModelFactory = bpViewModelFactory;
        }

        protected override void OnViewLoaded()
        {
            Init();
            if (WindowManager.ShowDialog(_webFileUpdaterViewModelFactory.CreateViewModel()) != true)
            {
                Application.Current.Shutdown();
                return;
            }
            WindowManager.ShowDialog(_bpViewModelFactory.CreateViewModel());
            Application.Current.Shutdown();
            base.OnViewLoaded();
        }

        private void Init()
        {
            try
            {
                var appSetting = Its.Configuration.Settings.Get<AppSetting>();
                var position = appSetting.Positions.SingleOrDefault(s => s.Width == (int)SystemParameters.PrimaryScreenWidth && s.Height == (int)SystemParameters.PrimaryScreenHeight);
                if (position == null)
                {
                    ShowMessageBox(L("MSG_NoMatchResolution"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Application.Current.Shutdown();
                    return;
                }
                App.MyPosition = position;
            }
            catch (Exception e)
            {
                ShowMessageBox(e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        public interface IBpViewModelFactory
        {
            BpViewModel CreateViewModel();
        }
        public interface IWebFileUpdaterViewModelFactory
        {
            WebFileUpdaterViewModel CreateViewModel();
        }
    }

}
