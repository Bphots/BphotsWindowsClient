using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GlobalHotKey;
using HotsBpHelper.HeroFinder;
using HotsBpHelper.Settings;
using HotsBpHelper.Utils;

using NAppUpdate.Framework;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Tasks;
using Stylet;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
using Point = System.Drawing.Point;

namespace HotsBpHelper.Pages
{
    public class ShellViewModel : ViewModelBase
    {

        private readonly IWebFileUpdaterViewModelFactory _webFileUpdaterViewModelFactory;

        private readonly IBpViewModelFactory _bpViewModelFactory;

        private readonly IImageUtil _imageUtil;

        private readonly HotKeyManager _hotKeyManager;

        private BpViewModel _bpViewModel;

        private bool _isLoaded;

        private Notifier _notificationManager;

        private bool _autoShowHideHelper;
        private bool _autoDetect;
        private bool _initializeReset;

        public bool AutoShowHideHelper
        {
            get { return _autoShowHideHelper; }
            set
            {
                if (_bpViewModel == null)
                    return;

                if (_bpViewModel.AutoShowHideHelper == value)
                    return;
                
                _bpViewModel.AutoShowHideHelper = value;
                if (SetAndNotify(ref _autoShowHideHelper, value))
                {
                    if (_initializeReset)
                    {
                        ResetHelper();
                        BpViewModelOnRemindBpMode(null, EventArgs.Empty);
                    }
                }
            }
        }

        public bool AutoDetect
        {
            get { return _autoDetect; }
            set
            {
                if (_bpViewModel == null)
                    return;

                if (_bpViewModel.IsAutoMode == value)
                    return;

                SetAndNotify(ref _autoDetect, value);

                _bpViewModel.IsAutoMode = value;
            }
        }

        public ShellViewModel(IWebFileUpdaterViewModelFactory webFileUpdaterViewModelFactory, IBpViewModelFactory bpViewModelFactory, IImageUtil imageUtil)
        {
            _webFileUpdaterViewModelFactory = webFileUpdaterViewModelFactory;
            _bpViewModelFactory = bpViewModelFactory;
            _imageUtil = imageUtil;
            _hotKeyManager = new HotKeyManager();
        }

        readonly MessageOptions _toastOptions = new MessageOptions
        {
            ShowCloseButton = false,
            FreezeOnMouseEnter = true,
            UnfreezeOnMouseLeave = false,
            NotificationClickAction = n =>
            {
                n.Close();
            }
        };

        protected override void OnViewLoaded()
        {
            using (Mutex mutex = new Mutex(false, "Global\\" + Const.HOTSBPHELPER_PROCESS_NAME))
            {
                if (!mutex.WaitOne(0, false))
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Application.Current.Shutdown();
                return;
            }
            if (!App.Debug)
            {
                // ²»ÊÇµ÷ÊÔÄ£Äâ,Ôò¼ì²é¸üÐÂ
                Update();
            }
            InitSettings();
            if (App.Language.Contains("en"))
                ExpandHeroPropertiesForLatin();

            RegisterHotKey();
            
            _bpViewModel = _bpViewModelFactory.CreateViewModel();
            
            // Ä¬ÈÏ½ûÓÃ×Ô¶¯ÏÔÒþ
            _isLoaded = true;
            base.OnViewLoaded();

            _bpViewModel.HideBrowser();
            WindowManager.ShowWindow(_bpViewModel);
            _bpViewModel.Hide();
            AutoShowHideHelper = true;
            AutoDetect = true;

            _bpViewModel.RemindDetectMode += BpViewModelOnRemindDetectMode;
            _bpViewModel.RemindBpStart += BpViewModelOnRemindGameStart;
            _bpViewModel.TurnOffAutoDetectMode += BpViewModelOnTurnOffAutoDetectMode;

            _notificationManager = new Notifier(cfg =>
            {
                cfg.PositionProvider = new PrimaryScreenPositionProvider(
                    corner: Corner.BottomRight,
                    offsetX: 5,
                    offsetY: 65);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(5),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(2));

                cfg.Dispatcher = Application.Current.Dispatcher;
                cfg.DisplayOptions.TopMost = true;
                cfg.DisplayOptions.Width = 250;
            });


            _notificationManager.ShowInformation(L("Started") + Environment.NewLine + L("StartedTips"), _toastOptions);

            _initializeReset = true;
            Task.Run(CheckFocusAsync).ConfigureAwait(false);
        }

        private void BpViewModelOnTurnOffAutoDetectMode(object sender, EventArgs e)
        {
            Execute.OnUIThread(() => AutoDetect = false);
        }

        private void BpViewModelOnRemindDetectMode(object sender, EventArgs eventArgs)
        {
            var onText = "已开启英雄识别" + Environment.NewLine + L("OcrModeOnToolTip");
            var offText = "已关闭英雄识别" + Environment.NewLine + L("OcrModeOffToolTip");
            if (AutoDetect)
            {
                _notificationManager.ClearMessages(offText);
                _notificationManager.ShowSuccess(onText, _toastOptions);
            }
            else
            {
                _notificationManager.ClearMessages(onText);
                _notificationManager.ShowInformation(offText, _toastOptions);
            }
        }

        private void BpViewModelOnRemindBpMode(object sender, EventArgs eventArgs)
        {
            var onText = "已开启比赛检测" + Environment.NewLine + L("StartedTips");
            var offText = "已关闭比赛检测" + Environment.NewLine + L("AutoBpScreenModeOnToolTip");
            if (_autoShowHideHelper)
            {
                _notificationManager.ClearMessages(offText);
                _notificationManager.ShowSuccess(onText, _toastOptions);
            }
            else
            {
                _notificationManager.ClearMessages(onText);
                _notificationManager.ShowInformation(offText, _toastOptions);
            }
        }
        
        private void BpViewModelOnRemindGameStart(object sender, EventArgs eventArgs)
        {
            if (_autoDetect)
            {
                _notificationManager.ShowSuccess("背锅助手正在运行" + Environment.NewLine + L("OcrModeOnToolTip"), _toastOptions);
            }
            else
            {
                _notificationManager.ShowSuccess("背锅助手正在运行" + Environment.NewLine + L("OcrModeOffToolTip"), _toastOptions);
            }
        }

        private void RegisterHotKey()
        {
            try
            {
                _hotKeyManager.Register(Key.B, ModifierKeys.Control | ModifierKeys.Shift);
                _hotKeyManager.Register(Key.C, ModifierKeys.Control | ModifierKeys.Shift);
                _hotKeyManager.Register(Key.M, ModifierKeys.Control | ModifierKeys.Shift);
                _hotKeyManager.Register(Key.N, ModifierKeys.Control | ModifierKeys.Shift);
                _hotKeyManager.Register(Key.L, ModifierKeys.Control | ModifierKeys.Shift);
                _hotKeyManager.Register(Key.R, ModifierKeys.Control | ModifierKeys.Shift);
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

        private async Task CheckFocusAsync()
        {
            int lastStatus = 0;
            while (true)
            {
                await Task.Delay(1000);
                if (_bpViewModel == null)
                    continue;

                var hwnd = Win32.GetForegroundWindow();
                var pid = Win32.GetWindowProcessID(hwnd);
                Process process = Process.GetProcessById(pid);
                bool inHotsGame = process.ProcessName.StartsWith(Const.HEROES_PROCESS_NAME);
                bool inHotsHelper = process.ProcessName.StartsWith(Const.HOTSBPHELPER_PROCESS_NAME) || process.ProcessName.StartsWith("iexplore");
                if (inHotsGame)
                {
                    if (OcrUtil.SuspendScanning && lastStatus != 1)
                    {
                        if (_bpViewModel.BpScreenLoaded)
                            Execute.OnUIThread(() => _bpViewModel.Show());
                        OcrUtil.SuspendScanning = false;
                        lastStatus = 1;
                    }
                }
                if (!inHotsHelper && !inHotsGame)
                {
                    if (!OcrUtil.SuspendScanning && lastStatus != 2)
                    {
                        if (_bpViewModel == null)
                            continue;

                        await Task.Delay(1000);
                        var hwnd2 = Win32.GetForegroundWindow();
                        var pid2 = Win32.GetWindowProcessID(hwnd2);
                        Process process2 = Process.GetProcessById(pid2);
                        bool inHotsGame2 = process.ProcessName.StartsWith(Const.HEROES_PROCESS_NAME);
                        bool inHotsHelper2 = process.ProcessName.StartsWith(Const.HOTSBPHELPER_PROCESS_NAME);
                        if (!inHotsHelper2 && !inHotsGame2 && _bpViewModel.BpScreenLoaded)
                            Execute.OnUIThread(() => _bpViewModel.Hide());

                        OcrUtil.SuspendScanning = true;
                        lastStatus = 2;
                    }
                }
            }
        }

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.B)
            {
                ManuallyShowHideHelper();
            }
            if (e.HotKey.Key == Key.M)
            {
                SwitchVisibility();
            }
            if (e.HotKey.Key == Key.R)
            {
                if (_bpViewModel != null)
                    AutoDetect = !_bpViewModel.IsAutoMode;
            }
            if (e.HotKey.Key == Key.N)
            {
                ResetHelper();
            }
            else if (e.HotKey.Key == Key.C)
            {
                string captureName = Path.Combine(App.AppPath, "Screenshots", DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".bmp");
                _imageUtil.CaptureScreen().Save(captureName);
            }
        }
        public void SwitchVisibility()
        {
            if (_bpViewModel == null)
                return;

            if (_bpViewModel.View.Visibility == Visibility.Visible)
            {
                _bpViewModel.Hide();
            }
            else
            {
                _bpViewModel.Show();
            }
        }

        public void ResetHelper()
        {
            _bpViewModel?.Hide(); 
            _bpViewModel?.Reset();
        }
        
        public void ManuallyShowHideHelper()
        {
            var wasAuto = AutoShowHideHelper;
            AutoShowHideHelper = false;
            if (!wasAuto)
                ToggleVisible(true);
        }

        private void ToggleVisible(bool clear)
        {
            if (!_isLoaded)
            {
                return;
            }
            Execute.OnUIThread(() =>
            {
                if (clear)
                {
                    _bpViewModel.CancelAllActiveScan();
                    _bpViewModel.Reset();
                }
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
                // TODO update success
                //_notificationManager.Show(new NotificationContent
                //{
                //    Title = L("UpdateFullText"),
                //    Message = L("HotsBpHelper"),
                //    Type = NotificationType.Information
                //}, expirationTime: TimeSpan.FromSeconds(5));
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

        private void ExpandHeroPropertiesForLatin()
        {
            App.MyPosition.HeroWidth = (int) (App.MyPosition.HeroWidth * 1.25);
            App.MyPosition.HeroHeight = (int) (App.MyPosition.HeroHeight * 1.25);
            App.MyPosition.Left.HeroPathPoints =
                new[]
                {
                    new Point(1, 1), new Point(1, (int) (0.0185*App.MyPosition.Height)),
                    new Point(App.MyPosition.HeroWidth, App.MyPosition.HeroHeight),
                    new Point(App.MyPosition.HeroWidth, App.MyPosition.HeroHeight - (int) (0.0165*App.MyPosition.Height))
                };
            App.MyPosition.Right.HeroPathPoints =
                new[]
                {
                    new Point(App.MyPosition.HeroWidth, 1),
                    new Point(App.MyPosition.HeroWidth, 1 + (int) (0.0185*App.MyPosition.Height)),
                    new Point(1, App.MyPosition.HeroHeight),
                    new Point(1, App.MyPosition.HeroHeight - (int) (0.0165*App.MyPosition.Height))
                };
            App.MyPosition.MapPosition = new MapPosition()
                {
                    Location = new Point((int)(App.MyPosition.Width / 2 - 0.25 * App.MyPosition.Height), 0),
                    Width = (int)(0.5 * App.MyPosition.Height),
                    Height = (int)(0.03563 * App.MyPosition.Height)
                };
        }

        private void InitSettings()
        {
            try
            {
                var appSetting = Its.Configuration.Settings.Get<AppSetting>();
                var screenSize = ScreenUtil.GetScreenResolution();
                
                if (App.DynamicPosition)
                {
                    App.MyPosition = new Position(screenSize.Width, screenSize.Height);

                    // TODO remove
                    if (App.MyPosition.Height == 1440 && App.MyPosition.Width == 3440)
                    {
                        App.MyPosition.Left.HeroName1 = new Point(19, 247);
                        App.MyPosition.Right.HeroName1 = new Point(3426, 252);
                    }
                    if (App.MyPosition.Height > 1800 && App.MyPosition.Width > 2700)
                    {
                        App.MyPosition.Left.HeroName1 = new Point(25, 313);
                        try
                        {
                        var text = File.ReadAllLines(@".\rightTempConfig1824.txt")[0];
                        var paramsTemp = text.Trim().Split(',');
                        App.MyPosition.Right.HeroName1 = new Point(int.Parse(paramsTemp[0]), int.Parse(paramsTemp[1]));
                        }
                        catch (Exception)
                        {
                            App.MyPosition.Right.HeroName1 = new Point(2717, 317);
                        }
                    }
                    if (App.MyPosition.Height == 1080 && App.MyPosition.Width == 1920)
                    {
                        App.MyPosition.Left.HeroName1 = new Point(15, 186);
                        try
                        {
                            var text = File.ReadAllLines(@".\rightTempConfig1080.txt")[0];
                            var paramsTemp = text.Trim().Split(',');
                            App.MyPosition.Right.HeroName1 = new Point(int.Parse(paramsTemp[0]),
                                int.Parse(paramsTemp[1]));
                        }
                        catch (Exception)
                        {
                            App.MyPosition.Right.HeroName1 = new Point(1907, 189);
                        }
                    }
                    return;
                }

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
            RequestClose();
            Application.Current.Shutdown();
        }

        protected override void OnClose()
        {
            _hotKeyManager.Dispose();
            _bpViewModel.OcrUtil?.Dispose();
            AutoShowHideHelper = false;
            base.OnClose();
        }

        public void ShowAbout()
        {
            Process.Start(Const.ABOUT_URL);
        }

        public void ShowHelp()
        {
            Process.Start(Const.HELP_URL);
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