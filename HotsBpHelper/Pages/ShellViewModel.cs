using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GlobalHotKey;
using HotsBpHelper.Settings;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Common;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Tasks;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly IWebFileUpdaterViewModelFactory _webFileUpdaterViewModelFactory;

        private readonly IBpViewModelFactory _bpViewModelFactory;

        private readonly HotKeyManager _hotKeyManager;

        private BpViewModel _bpViewModel;

        public ShellViewModel(IWebFileUpdaterViewModelFactory webFileUpdaterViewModelFactory, IBpViewModelFactory bpViewModelFactory)
        {
            _webFileUpdaterViewModelFactory = webFileUpdaterViewModelFactory;
            _bpViewModelFactory = bpViewModelFactory;
            _hotKeyManager = new HotKeyManager();
        }

        protected override void OnViewLoaded()
        {
            if (!App.Debug)
            {
                // 不是调试模拟,则检查更新
                Update();
            }
            InitSettings();
            RegisterHotKey();
            if (WindowManager.ShowDialog(_webFileUpdaterViewModelFactory.CreateViewModel()) != true)
            {
                Application.Current.Shutdown();
                return;
            }
            _bpViewModel = _bpViewModelFactory.CreateViewModel();
            WindowManager.ShowWindow(_bpViewModel);
            base.OnViewLoaded();
        }

        private void RegisterHotKey()
        {
            _hotKeyManager.Register(Key.B, ModifierKeys.Control | ModifierKeys.Shift);
            _hotKeyManager.KeyPressed += HotKeyManagerPressed;
        }

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            _bpViewModel.ToggleVisible();
        }

        private void Update()
        {
            UpdateManager updManager = UpdateManager.Instance;
            try
            {
                updManager.ReinstateIfRestarted();

                updManager.UpdateSource = new SimpleWebSource(Const.UPDATE_FEED_XML);
                try
                {
                    updManager.CheckForUpdates();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Checking updates exception.");
                    return;
                }
                Logger.Trace("Need updates files: {0}", updManager.UpdatesAvailable);
                if (updManager.UpdatesAvailable == 0) return;
                try
                {
                    updManager.PrepareUpdates();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Preparing updates exception.");
                    return;
                }
                ShowMessageBox(L("UpdatesAvailable"), MessageBoxButton.OK, MessageBoxImage.Information);
                try
                {
                    foreach (var updateTask in updManager.Tasks)
                    {
                        Logger.Trace(((FileUpdateTask)updateTask).LocalPath);
                    }
                    updManager.ApplyUpdates(true);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Applying updates exception.");
                    ShowMessageBox(string.Format(L("UpdatesFailed"), ex.Message), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            finally
            {
                updManager.CleanUp();
            }
        }

        private void InitSettings()
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

        public void Exit()
        {
            Application.Current.Shutdown();
        }

        protected override void OnClose()
        {
            _hotKeyManager.Dispose();
            base.OnClose();
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