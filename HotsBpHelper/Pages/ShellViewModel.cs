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
using HotsBpHelper.Factories;
using HotsBpHelper.HeroFinder;
using HotsBpHelper.Services;
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

using StatsFetcher;
using StyletIoC;
using Point = System.Drawing.Point;

namespace HotsBpHelper.Pages
{
    public class ShellViewModel : ViewModelBase
    {
        private ViewModelFactory _viewModelFactory;

        private readonly IImageUtil _imageUtil;

        private readonly HotKeyManager _hotKeyManager;

        private readonly IToastService _toastService;

        private BpViewModel _bpViewModel;

        private bool _isLoaded;

        private bool _autoShowHideHelper;
        private bool _autoDetect;
        private bool _initializeReset;
        
        private MMRViewModel _mmrViewModel;
        
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
        private bool _autoShowMMR;

        public bool AutoShowMmr
        {
            get { return _autoShowMMR; }
            set
            {
                if (SetAndNotify(ref _autoShowMMR, value))
                {
                    if (AutoShowMmr)
                    {
                        Task.Run(() =>
                        {
                            MonitorLobbyFile();
                        });
                    }
                }
            }
        }

        public ShellViewModel(ViewModelFactory viewModelFactory, IImageUtil imageUtil, IToastService toastService)
        {
            _viewModelFactory = viewModelFactory;
            
            _imageUtil = imageUtil;
            _toastService = toastService;
            _hotKeyManager = new HotKeyManager();
        }
        
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
                // ¬≤¬ª√ä√á¬µ√∑√ä√î√Ñ¬£√Ñ√¢,√î√≤¬º√¨¬≤√©¬∏√º√ê√Ç
                // Update();
            }
            InitSettings();

            RegisterHotKey();
            if (!App.Debug && WindowManager.ShowDialog(_viewModelFactory.CreateViewModel<WebFileUpdaterViewModel>()) != true)
            {
                Application.Current.Shutdown();
                return;
            }

            _bpViewModel = _viewModelFactory.CreateViewModel<BpViewModel>(); 

            // √Ñ¬¨√à√è¬Ω√ª√ì√É√ó√î¬∂¬Ø√è√î√í√æ
            _isLoaded = true;
            base.OnViewLoaded();

            _bpViewModel.HideBrowser();
            WindowManager.ShowWindow(_bpViewModel);
            _bpViewModel.Hide();


            _mmrViewModel = _viewModelFactory.CreateViewModel<MMRViewModel>(); 
            WindowManager.ShowWindow(_mmrViewModel);

            AutoShowHideHelper = true;
            NotifyOfPropertyChange(() => CanOcr);
            if (!CanOcr)    
                _toastService.ShowWarning(L("LanguageUnavailable"));
            if (App.AppSetting.Position.Height < 1070)
                _toastService.ShowWarning(L("IncompatibleResolution"));
            
            AutoDetect = _bpViewModel.OcrAvailable;
            AutoShowMmr = true; // ƒ¨»œ∆Ù”√◊‘∂Øœ‘ æMMR

            _bpViewModel.RemindDetectMode += BpViewModelOnRemindDetectMode;
            _bpViewModel.RemindBpStart += BpViewModelOnRemindGameStart;
            _bpViewModel.TurnOffAutoDetectMode += BpViewModelOnTurnOffAutoDetectMode;

            _toastService.ShowInformation(L("Started") + Environment.NewLine + L("StartedTips"));

            _initializeReset = true;
            Task.Run(CheckFocusAsync).ConfigureAwait(false);
            Task.Run(MonitorInGameAsync).ConfigureAwait(false);
        }

        public bool CanOcr => _bpViewModel != null && _bpViewModel.OcrAvailable;

        private void BpViewModelOnTurnOffAutoDetectMode(object sender, EventArgs e)
        {
            Execute.OnUIThread(() => AutoDetect = false);
        }

        private void BpViewModelOnRemindDetectMode(object sender, EventArgs eventArgs)
        {
            var onText = L("OcrModeOnTitle") + Environment.NewLine + L("OcrModeOnToolTip");
            var offText = L("OcrModeOffTitle") + Environment.NewLine + L("OcrModeOffToolTip");
            if (AutoDetect)
            {
                _toastService.CloseMessages(offText);
                _toastService.ShowSuccess(onText);
            }
            else
            {
                _toastService.CloseMessages(onText);
                _toastService.ShowInformation(offText);
            }
        }

        private void BpViewModelOnRemindBpMode(object sender, EventArgs eventArgs)
        {
            var onText = L("MatchDetectOnTitle") + Environment.NewLine + L("StartedTips");
            var offText = L("MatchDetectOffTitle") + Environment.NewLine + L("AutoBpScreenModeOnToolTip");
            if (_autoShowHideHelper)
            {
                _toastService.CloseMessages(offText);
                _toastService.ShowSuccess(onText);
            }
            else
            {
                _toastService.CloseMessages(onText);
                _toastService.ShowInformation(offText);
            }
        }
        
        private void BpViewModelOnRemindGameStart(object sender, EventArgs eventArgs)
        {
            if (_autoDetect)
            {
                _toastService.ShowSuccess(L("OcrModeToolTipTitle") + Environment.NewLine + L("OcrModeOnToolTip"));
            }
            else
            {
                _toastService.ShowSuccess(L("OcrModeToolTipTitle") + Environment.NewLine + L("OcrModeOffToolTip"));
            }
        }
        
      private void MonitorLobbyFile()
        {
            DateTime lobbyLastModified = DateTime.MinValue;
            while (AutoShowMmr)
            {
                if (File.Exists(Const.BattleLobbyPath) && File.GetLastWriteTime(Const.BattleLobbyPath) != lobbyLastModified)
                {
                    lobbyLastModified = File.GetLastWriteTime(Const.BattleLobbyPath);
                    var game = FileProcessor.ProcessLobbyFile(Const.BattleLobbyPath);
                    _mmrViewModel.FillMMR(game);
                    Execute.OnUIThread(() =>
                    {
                        _mmrViewModel.View.Visibility = Visibility.Visible;
                        _bpViewModel.Reset();
                    });
                }
                Thread.Sleep(1000);
            }
        }

        private async Task MonitorInGameAsync()
        {
            while (true)
            {
                if (File.Exists(Const.BattleLobbyPath) && !OcrUtil.InGame)
                {
                    OcrUtil.InGame = true;
                }

                if (!File.Exists(Const.BattleLobbyPath) && OcrUtil.InGame)
                {
                    OcrUtil.InGame = false;
                }

                await Task.Delay(1000);
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
                        OcrUtil.NotInFocus = false;
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
                        bool inHotsGame2 = process2.ProcessName.StartsWith(Const.HEROES_PROCESS_NAME);
                        bool inHotsHelper2 = process2.ProcessName.StartsWith(Const.HOTSBPHELPER_PROCESS_NAME);
                        if (!inHotsHelper2 && !inHotsGame2 && _bpViewModel.BpScreenLoaded)
                            Execute.OnUIThread(() => _bpViewModel.Hide());

                        OcrUtil.NotInFocus = true;
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
                if (CanOcr)
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

                _toastService.ShowInformation(L("UpdateFullText") + Environment.NewLine + L("HotsBpHelper"));

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
            App.AppSetting.Position.HeroWidth = (int) (App.AppSetting.Position.HeroWidth * 1.25);
            App.AppSetting.Position.HeroHeight = (int) (App.AppSetting.Position.HeroHeight * 1.25);
            App.AppSetting.Position.Left.HeroPathPoints =
                new[]
                {
                    new Point(1, 1), new Point(1, (int) (0.0185*App.AppSetting.Position.Height)),
                    new Point(App.AppSetting.Position.HeroWidth, App.AppSetting.Position.HeroHeight),
                    new Point(App.AppSetting.Position.HeroWidth, App.AppSetting.Position.HeroHeight - (int) (0.0165*App.AppSetting.Position.Height))
                };
            App.AppSetting.Position.Right.HeroPathPoints =
                new[]
                {
                    new Point(App.AppSetting.Position.HeroWidth, 1),
                    new Point(App.AppSetting.Position.HeroWidth, 1 + (int) (0.0185*App.AppSetting.Position.Height)),
                    new Point(1, App.AppSetting.Position.HeroHeight),
                    new Point(1, App.AppSetting.Position.HeroHeight - (int) (0.0165*App.AppSetting.Position.Height))
                };
            App.AppSetting.Position.MapPosition = new MapPosition()
                {
                    Location = new Point((int)(App.AppSetting.Position.Width / 2 - 0.25 * App.AppSetting.Position.Height), 0),
                    Width = (int)(0.5 * App.AppSetting.Position.Height),
                    Height = (int)(0.03563 * App.AppSetting.Position.Height)
                };
        }
        
        private void InitSettings()
        {
            try
            {
                App.AppSetting = Its.Configuration.Settings.Get<AppSetting>();
                var screenSize = ScreenUtil.GetScreenResolution();
                App.AppSetting.Position = CaculatePosition(screenSize.Width, screenSize.Height);
                
                // TODO dynamic support
                ManualAdjustPosition();

                if (App.Language.Contains("en"))
                    ExpandHeroPropertiesForLatin();
            }
            catch (Exception e)
            {
                Pages.ErrorView _errorView = new Pages.ErrorView(e.Message);
                _errorView.ShowDialog();
                _errorView.Pause();
            }
        }

        private static void ManualAdjustPosition()
        {
            if (App.AppSetting.Position.Height == 1440 && App.AppSetting.Position.Width == 3440)
            {
                App.AppSetting.Position.Left.HeroName1 = new Point(19, 247);
                App.AppSetting.Position.Right.HeroName1 = new Point(3426, 252);
                App.AppSetting.Position.OverlapPoints = new OverlapPoints
                {
                    AppearanceFramePoint = new Point(81, 521),
                    FrameRightBorderPoint = new Point(3423, 1241),
                    SkillFramePoint = new Point(121, 1111),
                    TalentFramePoint = new Point(213, 425),
                    FullChatHorizontalPoint = new Point(1960, 331),
                    PartialChatlHorizontalPoint = new Point(1960, 842)
                };
            }
            if (App.AppSetting.Position.Height > 1800 && App.AppSetting.Position.Width > 2700)
            {
                App.AppSetting.Position.Left.HeroName1 = new Point(25, 313);
                App.AppSetting.Position.Right.HeroName1 = new Point(2717, 317);
            }
            if (App.AppSetting.Position.Height == 1080 && App.AppSetting.Position.Width == 1920)
            {
                App.AppSetting.Position.Left.HeroName1 = new Point(15, 186);
                App.AppSetting.Position.Right.HeroName1 = new Point(1907, 189);
                App.AppSetting.Position.OverlapPoints = new OverlapPoints
                {
                    AppearanceFramePoint = new Point(70, 457),
                    FrameRightBorderPoint = new Point(1907, 938),
                    SkillFramePoint = new Point(77, 833),
                    TalentFramePoint = new Point(147, 319),
                    FullChatHorizontalPoint = new Point(1173, 248),
                    PartialChatlHorizontalPoint = new Point(1173, 632)
                };
            }


        }

        /// <summary>
        /// ∏˘æ›∑÷±Ê¬ ∂ØÃ¨º∆À„∏˜∏ˆŒª÷√∫Õ≥ﬂ¥Á
        /// </summary>
        private Position CaculatePosition(int width, int height)
        {
            var bpHelperSize = App.AppSetting.DefaultBpHelperSize;
            int heroWidth = (int)(0.08125 * height);
            var heroHeight = (int)(0.0632 * height);
            var position = new Position
            {
                Width = width,
                Height = height,
                BpHelperSize = bpHelperSize,
                BpHelperPosition = new Point((int) (0.31*height),
                    0.852*height + bpHelperSize.Height > height ? (height - bpHelperSize.Height) : (int) (0.852*height)),
                MapSelectorPosition = new Point((int) (0.5*width), (int) (0.146*height)),
                HeroWidth = heroWidth,
                HeroHeight = heroHeight,
                Left = new SidePosition
                {
                    Ban1 = new Point((int)(0.45 * height), (int)(0.016 * height)),
                    Ban2 = new Point((int)(0.45 * height), (int)(0.016 * height) + (int)(0.023 * height) + (int)(0.015 * height)),
                    Pick1 = new Point((int)(0.195 * height), (int)(0.132 * height)),
                    Dx = (int)(0.0905 * height),
                    Dy = (int)(0.1565 * height),
                    HeroPathPoints =
                    new[]
                    {
                        new Point(1, 1), new Point(1, (int) (0.0185*height)),
                        new Point(heroWidth, heroHeight),
                        new Point(heroWidth, heroHeight - (int) (0.0165*height))
                    },
                    HeroName1 = new Point((int)(0.013195 * height), (int)(0.172222 * height))
                },
                Right = new SidePosition
                {
                    Ban1 = new Point((int)(width - 0.45 * height), (int)(0.016 * height)),
                    Ban2 = new Point((int)(width - 0.45 * height), (int)(0.016 * height) + (int)(0.023 * height) + (int)(0.015 * height)),
                    Pick1 = new Point((int)(width - 0.195 * height), (int)(0.132 * height)),
                    Dx = (int)(-0.0905 * height),
                    Dy = (int)(0.1565 * height),
                    HeroPathPoints =
                    new[]
                    {
                        new Point(heroWidth, 1), new Point(heroWidth, 1 + (int) (0.0185*height)),
                        new Point(1, heroHeight),
                        new Point(1, heroHeight - (int) (0.0165*height))
                    },
                    HeroName1 = new Point((int)(width - 0.011195 * height), (int)(0.172222 * height))
                },
                MapPosition = new MapPosition()
                {
                    Location = new Point((int)(width / 2 - 0.18 * height), 0),
                    Width = (int)(0.36 * height),
                    Height = (int)(0.03563 * height)
                }
            };
            return position;
        }

        public void Exit()
        {
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

    }

}