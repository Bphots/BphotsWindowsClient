using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DMDotNet;
using GlobalHotKey;
using HotsBpHelper.Settings;
using HotsBpHelper.Utils;
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

        private bool isLoaded = false;

        private Form1 form1 = new Form1();

        private NDM _ndm;

        private bool _autoShowHideHelper;
        public bool AutoShowHideHelper
        {
            get { return _autoShowHideHelper; }
            set
            {
                if (SetAndNotify(ref _autoShowHideHelper, value))
                {
                    if (AutoShowHideHelper)
                    {
                        Task.Run(() =>
                        {
                            using (_ndm = new NDM())
                            {
                                CheckBpUi();
                            }
                        });
                    }
                }
            }
        }

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
            form1.kill();
            AutoShowHideHelper = true;
            isLoaded = true;
            base.OnViewLoaded();
        }

        private void CheckBpUi()
        {

            _ndm.SetPath(Path.Combine(App.AppPath, @"Images\lock"));
            string picNames = _ndm.MatchPicName("*.bmp");
            while (AutoShowHideHelper)
            {
                string result = _ndm.DM.FindPicEx(0, 0, App.MyPosition.Width, 300, picNames, "000000", 0.8, 0);
                if (result != String.Empty && _bpViewModel.View?.Visibility == Visibility.Hidden ||
                    result == String.Empty && _bpViewModel.View?.Visibility == Visibility.Visible)
                {
                    ToggleVisible();
                }
                Thread.Sleep(1500);
            }
        }

        private void RegisterHotKey()
        {
            try
            {
                _hotKeyManager.Register(Key.B, ModifierKeys.Control | ModifierKeys.Shift);
                _hotKeyManager.Register(Key.C, ModifierKeys.Control | ModifierKeys.Shift);
                _hotKeyManager.KeyPressed += HotKeyManagerPressed;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                /*
                Pages.ErrorView _errorView = new Pages.ErrorView(L("RegisterHotKeyFailed"),e.Message);
                _errorView.isShutDown = false;
                _errorView.ShowDialog();
                */
                ShowMessageBox(L("RegisterHotKeyFailed"), MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
        }

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.B)
            {
                AutoShowHideHelper = !AutoShowHideHelper;
            }
            else if (e.HotKey.Key == Key.C)
            {
                string captureName = Path.Combine(App.AppPath, "Screenshots", DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".bmp");
                _ndm.Capture(0, 0, App.MyPosition.Width, App.MyPosition.Height, captureName);
            }
        }

        private void ToggleVisible()
        {
            if (!isLoaded)
            {
                return;
            }
            Execute.OnUIThread(() =>
            {
                _bpViewModel.Init();
                _bpViewModel.Reload();
                _bpViewModel.ToggleVisible();
            });
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
                //ShowMessageBox(L("UpdatesAvailable"), MessageBoxButton.OK, MessageBoxImage.Information);

                form1.ShowBallowNotify();
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
                var screenSize = ScreenUtil.GetScreenResolution();
                var position = appSetting.Positions.SingleOrDefault(s => s.Width == (int)screenSize.Width && s.Height == (int)screenSize.Height);
                if (position == null)
                {
                    Pages.ErrorView _errorView = new Pages.ErrorView(L("NoMatchResolution"), L("MSG_NoMatchResolution"));
                    _errorView.ShowDialog();
                    _errorView.isShutDown = true;
                    _errorView.Pause();
                    //ShowMessageBox(L("MSG_NoMatchResolution"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //Application.Current.Shutdown();
                    return;
                }
                App.MyPosition = position;
            }
            catch (Exception e)
            {
                Pages.ErrorView _errorView = new Pages.ErrorView(e.Message);
                _errorView.ShowDialog();
                _errorView.Pause();
            }
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }

        protected override void OnClose()
        {
            _hotKeyManager.Dispose();
            AutoShowHideHelper = false;
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