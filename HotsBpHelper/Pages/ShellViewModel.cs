using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Chromium;
using GlobalHotKey;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Security;
using HotsBpHelper.Factories;
using HotsBpHelper.Services;
using HotsBpHelper.Settings;
using HotsBpHelper.UserControls;
using HotsBpHelper.Utils;
using HotsBpHelper.WPF;
using ImageProcessor.Ocr;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Tasks;
using StatsFetcher;
using Stylet;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;

namespace HotsBpHelper.Pages
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly HotKeyManager _hotKeyManager;

        private readonly IImageUtil _imageUtil;
        private NotifyTaskCompletion<double> _notifyGetTimeStampTaskCompleted;
        private readonly ISecurityProvider _securityProvider;

        private readonly IToastService _toastService;
        private bool _autoDetect;

        private bool _autoShowHideHelper;
        private bool _autoShowMMR;

        private BpViewModel _bpViewModel;
        private bool _initializeReset;

        private bool _isLoaded;

        private MMRViewModel _mmrViewModel;
        private string _percentageInfo;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly IRestApi _restApi;
        private NotifyTaskCompletion<bool> _notifyUpdateTaskCompleted;
        private bool _canOcr;

        public ShellViewModel(ViewModelFactory viewModelFactory, IImageUtil imageUtil, IToastService toastService,
            IRestApi restApi, ISecurityProvider securityProvider)
        {
            _viewModelFactory = viewModelFactory;

            _imageUtil = imageUtil;
            _toastService = toastService;
            _securityProvider = securityProvider;
            _restApi = restApi;

            _hotKeyManager = new HotKeyManager();
            CefInitializer.InitializeCef();

            PercentageInfo = L("Loading");


            using (var mutex = new Mutex(false, "Global\\" + Const.HOTSBPHELPER_PROCESS_NAME))
            {
                if (!mutex.WaitOne(0, false))
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Exit();
                return;
            }
        }

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

                if (value)
                {
                    if (!OcrEngine.IsTessDataAvailable(App.OcrLanguage))
                    {
                        IsLoaded = false;
                        if (TopMostMessageBox.Show(L("TessdataQuestion"), @"Warning",
                         MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            var tessdataWebUpdateVm = _viewModelFactory.CreateViewModel<WebFileUpdaterViewModel>();
                            var languageParams = OcrEngine.GetDirectory(App.OcrLanguage);
                            tessdataWebUpdateVm.ShellViewModel = this;
                            tessdataWebUpdateVm.UpdateCompleted += OnTessdataFileReinitializeCompleted;
                            tessdataWebUpdateVm.SetPaths(languageParams[0], languageParams[1]);
                            WindowManager.ShowDialog(tessdataWebUpdateVm);
                        }
                        else
                        {
                            IsLoaded = true;
                            return;
                        }
                    }
                }

                SetAndNotify(ref _autoDetect, value);

                _bpViewModel.IsAutoMode = value;
            }
        }

        private void OnTessdataFileReinitializeCompleted(object sender, EventArgs e)
        {
            _bpViewModel.ReInitializeOcr();
            IsLoaded = true;
        }

        public bool AutoShowMmr
        {
            get { return _autoShowMMR; }
            set
            {
                SetAndNotify(ref _autoShowMMR, value);
            }
        }

        public string PercentageInfo
        {
            get { return _percentageInfo; }
            set { SetAndNotify(ref _percentageInfo, value); }
        }

        public bool CanOcr { get { return _canOcr; } set { SetAndNotify(ref _canOcr, value); } }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetAndNotify(ref _isLoaded, value); }
        }

        private void OnTimeStampCompleted(object sender, EventArgs e)
        {
            if (!_notifyGetTimeStampTaskCompleted.IsSuccessfullyCompleted)
            {
                Exit();
                return;
            }

            ReceiveBroadcast();

            if (!App.Debug)
            {
                // 虏禄脢脟碌梅脢脭脛拢脛芒,脭貌录矛虏茅赂眉脨脗
                _notifyUpdateTaskCompleted = new NotifyTaskCompletion<bool>(UpdateAsync());
                _notifyUpdateTaskCompleted.TaskStopped += OnFeedUpdateCompleted;
                if (_notifyUpdateTaskCompleted.IsCompleted)
                    OnFeedUpdateCompleted(this, EventArgs.Empty);
            }
            else
                OnFeedUpdateCompleted(this, EventArgs.Empty);
        }

        protected override void OnViewLoaded()
        {
            _notifyGetTimeStampTaskCompleted = new NotifyTaskCompletion<double>(_restApi.GetTimestamp());
            _notifyGetTimeStampTaskCompleted.TaskStopped += OnTimeStampCompleted;
            if (_notifyGetTimeStampTaskCompleted.IsCompleted)
                OnTimeStampCompleted(this, EventArgs.Empty);
            
            InitSettings();
            
            base.OnViewLoaded();

            _toastService.ShowInformation(L("Loading"));
        }

        private void ReceiveBroadcast()
        {
            //接收公告，并以对话框的形式显示
            var broadcastList = _restApi.GetBroadcastInfo("0", App.Language);
            if (broadcastList != null)
            {
                foreach (var broadcast in broadcastList)
                {
                    if (broadcast.Type == 0)
                    {
                        var b = new BroadcastWindow(broadcast.Msg, broadcast.Url);
                        b.Show();
                    }
                }
                foreach (var broadcast in broadcastList)
                {
                    if (broadcast.Type == 1)
                    {
                        var e = new ErrorView(L("Reminder"), broadcast.Msg, broadcast.Url);
                        e.ShowDialog();
                    }
                }
            }
        }

        private void OnFeedUpdateCompleted(object sender, EventArgs e)
        {
            if (_notifyUpdateTaskCompleted != null && !_notifyUpdateTaskCompleted.IsSuccessfullyCompleted)
            {
                Exit();
                return;
            }

            Execute.OnUIThread(() =>
            {
                _securityProvider.SetServerTimestamp(_notifyGetTimeStampTaskCompleted.Result);
                if (!App.Debug)
                {
                    var webUpdateVm = _viewModelFactory.CreateViewModel<WebFileUpdaterViewModel>();
                    webUpdateVm.ShellViewModel = this;
                    webUpdateVm.UpdateCompleted += OnWebFileUpdateCompleted;
                    if (WindowManager.ShowDialog(webUpdateVm) != true)
                        Exit();
                }
                else
                    OnWebFileUpdateCompleted(this, EventArgs.Empty);
            });
        }


        private void OnTessdataFileUpdateCompleted(object sender, EventArgs e)
        {
            _mmrViewModel = _viewModelFactory.CreateViewModel<MMRViewModel>();
            _mmrViewModel.HideBrowser();
            WindowManager.ShowWindow(_mmrViewModel);
            _mmrViewModel.Hide();

            _bpViewModel = _viewModelFactory.CreateViewModel<BpViewModel>();
            WindowManager.ShowWindow(_bpViewModel);
            _bpViewModel.Hide();

            RegisterHotKey();
            IsLoaded = true;
            AutoShowHideHelper = true;

            CanOcr = true;
            if (!_bpViewModel.OcrAvailable)
                _toastService.ShowWarning(L("LanguageUnavailable"));

            if (App.AppSetting.Position.Height < 1070)
            {
                CanOcr = false;
                _toastService.ShowWarning(L("IncompatibleResolution"));
            }

            if (App.OcrLanguage == OcrLanguage.Korean)
                CanOcr = false;

            AutoDetect = CanOcr && _bpViewModel.OcrAvailable;
            AutoShowMmr = true; // 默认启用自动显示MMR

            _bpViewModel.RemindDetectMode += BpViewModelOnRemindDetectMode;
            _bpViewModel.RemindBpStart += BpViewModelOnRemindGameStart;
            _bpViewModel.TurnOffAutoDetectMode += BpViewModelOnTurnOffAutoDetectMode;

            _toastService.CloseMessages(L("Loading"));
            _toastService.ShowInformation(L("Started") + Environment.NewLine + L("StartedTips"));

            _initializeReset = true;
            Task.Run(CheckFocusAsync).ConfigureAwait(false);
            Task.Run(MonitorInGameAsync).ConfigureAwait(false);
            Task.Run(MonitorLobbyFile).ConfigureAwait(false);
        }

        private void OnWebFileUpdateCompleted(object sender, EventArgs e)
        {
            if (App.OcrLanguage == OcrLanguage.Korean)
                OnTessdataFileUpdateCompleted(this, EventArgs.Empty);

            var tessdataWebUpdateVm = _viewModelFactory.CreateViewModel<WebFileUpdaterViewModel>();
            tessdataWebUpdateVm.ShellViewModel = this;
            tessdataWebUpdateVm.UpdateCompleted += OnTessdataFileUpdateCompleted;

            var languageParams = OcrEngine.GetDirectory(App.OcrLanguage);
            if (!OcrEngine.IsTessDataAvailable(App.OcrLanguage))
            {
                if (TopMostMessageBox.Show(L("TessdataQuestion"), @"Warning",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    tessdataWebUpdateVm.SetPaths(languageParams[0], languageParams[1]);
                    WindowManager.ShowDialog(tessdataWebUpdateVm);
                }
                else
                    OnTessdataFileUpdateCompleted(this, EventArgs.Empty);
            }
            else
            {
                tessdataWebUpdateVm.SetPaths(languageParams[0], languageParams[1]);
                WindowManager.ShowDialog(tessdataWebUpdateVm);
            }
        }

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

        private async Task MonitorLobbyFile()
        {
            var lobbyLastModified = DateTime.MinValue;
            while (AutoShowMmr)
            {
                if (File.Exists(Const.BattleLobbyPath) &&
                    File.GetLastWriteTime(Const.BattleLobbyPath) != lobbyLastModified)
                {
                    lobbyLastModified = File.GetLastWriteTime(Const.BattleLobbyPath);
                    var game = FileProcessor.ProcessLobbyFile(Const.BattleLobbyPath);
                    _mmrViewModel.FillMMR(game);
                    Execute.OnUIThread(() =>
                    {
                        _mmrViewModel.Show();
                        _bpViewModel.Reset();
                    });
                }
                await Task.Delay(1000);
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
                _hotKeyManager.Register(Key.L, ModifierKeys.Control | ModifierKeys.Shift);
                _hotKeyManager.Register(Key.R, ModifierKeys.Control | ModifierKeys.Shift);
                _hotKeyManager.Register(Key.D, ModifierKeys.Control | ModifierKeys.Shift);
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
                ShowMessageBox(L("RegisterHotKeyFailed"), MessageBoxButton.OK, MessageBoxImage.Exclamation,
                    MessageBoxResult.OK);
            }
        }

        private async Task CheckFocusAsync()
        {
            var lastStatus = 0;
            bool hotsClosed = true;
            while (true)
            {
                await Task.Delay(1000);
                if (_bpViewModel == null)
                    continue;

                var hwnd = Win32.GetForegroundWindow();
                var pid = Win32.GetWindowProcessID(hwnd);
                var process = Process.GetProcessById(pid);
                var hotsProcess = Process.GetProcessesByName(Const.HEROES_PROCESS_NAME).Union(Process.GetProcessesByName(Const.HEROES_PROCESS_NAME + "_x64"));
                if (!hotsProcess.Any())
                {
                    if (!OcrUtil.NotInFocus && lastStatus != 2)
                    {
                        OcrUtil.NotInFocus = true;
                        lastStatus = 2;
                    }

                    if (!hotsClosed && _bpViewModel.BpScreenLoaded)
                    {
                        Execute.OnUIThread(() =>
                        {
                            _bpViewModel.Reset();
                        });
                        hotsClosed = true;
                    }
                    continue;
                }

                hotsClosed = false;

                var inHotsGame = process.ProcessName.StartsWith(Const.HEROES_PROCESS_NAME);
                var inHotsHelper = process.ProcessName.StartsWith(Const.HOTSBPHELPER_PROCESS_NAME);
                if (inHotsGame)
                {
                    if (OcrUtil.NotInFocus && lastStatus != 1)
                    {
                        if (_bpViewModel.BpScreenLoaded)
                            Execute.OnUIThread(() => _bpViewModel.Show());
                        OcrUtil.NotInFocus = false;
                        lastStatus = 1;
                    }
                }
                if (!inHotsHelper && !inHotsGame)
                {
                    if (!OcrUtil.NotInFocus && lastStatus != 2)
                    {
                        if (_bpViewModel == null)
                            continue;

                        await Task.Delay(1000);
                        var hwnd2 = Win32.GetForegroundWindow();
                        var pid2 = Win32.GetWindowProcessID(hwnd2);
                        var process2 = Process.GetProcessById(pid2);
                        var inHotsGame2 = process2.ProcessName.StartsWith(Const.HEROES_PROCESS_NAME);
                        var inHotsHelper2 = process2.ProcessName.StartsWith(Const.HOTSBPHELPER_PROCESS_NAME);
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
            if (!IsLoaded)
                return;
            
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
            if (e.HotKey.Key == Key.D)
            {
                if (_bpViewModel == null)
                    return; 

                _bpViewModel.ShowDevTool = !_bpViewModel.ShowDevTool;
            }
            else if (e.HotKey.Key == Key.C)
            {
                var captureName = Path.Combine(App.AppPath, "Screenshots",
                    DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".bmp");
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
                    _bpViewModel.Reset(false);
                }
                _bpViewModel.ToggleVisible();
            });
        }

        private async Task<bool> UpdateAsync()
        {
            await Task.Run(() => Update());
            return true;
        }

        private void Update()
        {
            var updManager = UpdateManager.Instance;
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

                Execute.OnUIThread(() => _toastService.ShowInformation(L("UpdateFullText") + Environment.NewLine + L("HotsBpHelper")));

                try
                {
                    updManager.PrepareUpdates();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Preparing updates exception.");
                    return;
                }

                try
                {
                    foreach (var updateTask in updManager.Tasks)
                    {
                        Logger.Trace(((FileUpdateTask) updateTask).LocalPath);
                    }
                    updManager.ApplyUpdates(true);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Applying updates exception.");
                    ShowMessageBox(string.Format(L("UpdatesFailed"), ex.Message), MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            finally
            {
                updManager.CleanUp();
            }
        }

        private void ExpandHeroPropertiesForLatin()
        {
            App.AppSetting.Position.HeroWidth = (int) (App.AppSetting.Position.HeroWidth*1.25);
            App.AppSetting.Position.HeroHeight = (int) (App.AppSetting.Position.HeroHeight*1.25);
            App.AppSetting.Position.Left.HeroPathPoints =
                new[]
                {
                    new Point(1, 1), new Point(1, (int) (0.0185*App.AppSetting.Position.Height)),
                    new Point(App.AppSetting.Position.HeroWidth, App.AppSetting.Position.HeroHeight),
                    new Point(App.AppSetting.Position.HeroWidth,
                        App.AppSetting.Position.HeroHeight - (int) (0.0165*App.AppSetting.Position.Height))
                };
            App.MyPosition.Right.HeroPathPoints =
                new[]
                {
                    new Point(App.MyPosition.HeroWidth, 1),
                    new Point(App.MyPosition.HeroWidth, 1 + (int) (0.0185*App.MyPosition.Height)),
                    new Point(1, App.MyPosition.HeroHeight),
                    new Point(1, App.MyPosition.HeroHeight - (int) (0.0165*App.MyPosition.Height))
                };
            App.AppSetting.Position.MapPosition = new MapPosition
            {
                Location = new Point((int) (App.AppSetting.Position.Width/2 - 0.25*App.AppSetting.Position.Height), 0),
                Width = (int) (0.5*App.AppSetting.Position.Height),
                Height = (int) (0.03563*App.AppSetting.Position.Height)
            };
        }

        private void InitSettings()
        {
            try
            {
                var appSetting = Its.Configuration.Settings.Get<AppSetting>();
                var screenSize = ScreenUtil.GetScreenResolution();
                App.AppSetting.Position = CaculatePosition(screenSize.Width, screenSize.Height);

                // TODO dynamic support
                ManualAdjustPosition();

                if (App.Language.Contains("en"))
                    ExpandHeroPropertiesForLatin();
            }
            catch (Exception e)
            {
                var _errorView = new ErrorView(e.Message);
                _errorView.ShowDialog();
                _errorView.Pause();
            }
        }

        private static void ManualAdjustPosition()
        {
            //var height = App.AppSetting.Position.Height;
            //var width = App.AppSetting.Position.Width;
            //if (App.AppSetting.Position.Height == 1440 && App.AppSetting.Position.Width == 3440)
            //{
            //    App.AppSetting.Position.Left.HeroName1 = new Point(19, 247);
            //    App.AppSetting.Position.Right.HeroName1 = new Point(3426, 252);
            //}
            //if (App.AppSetting.Position.Height > 1800 && App.AppSetting.Position.Width > 2700)
            //{
            //    App.AppSetting.Position.Left.HeroName1 = new Point(25, 313);
            //    App.AppSetting.Position.Right.HeroName1 = new Point(2717, 317);
            //}
            //if (App.AppSetting.Position.Height == 1080 && App.AppSetting.Position.Width == 1920)
            //{
            //    App.AppSetting.Position.Left.HeroName1 = new Point(RoundUp(0.0138888888888889 * height), RoundUp(0.1722222222222222 * height));
            //    App.AppSetting.Position.Right.HeroName1 = new Point(RoundUp(width - 0.012037037037037 * height), RoundUp(0.0175 * height)); 
            //}
        }

        /// <summary>
        ///     根据分辨率动态计算各个位置和尺寸
        /// </summary>
        private Position CaculatePosition(int width, int height)
        {
            var bpHelperSize = App.AppSetting.DefaultBpHelperSize;
            var heroWidth = (int) (0.08125*height);
            var heroHeight = (int) (0.0632*height);
            var position = new Position
            {
                Width = width,
                Height = height,
                BpHelperSize = bpHelperSize,
                BpHelperPosition = new Point((int) (0.31*height),
                    0.852*height + bpHelperSize.Height > height ? height - bpHelperSize.Height : (int) (0.852*height)),
                MapSelectorPosition = new Point((int) (0.5*width), (int) (0.146*height)),
                HeroWidth = heroWidth,
                HeroHeight = heroHeight,
                Left = new SidePosition
                {
                    Ban1 = new Point((int) (0.45*height), (int) (0.016*height)),
                    Ban2 =
                        new Point((int) (0.45*height),
                            (int) (0.016*height) + (int) (0.023*height) + (int) (0.015*height)),
                    Pick1 = new Point((int) (0.195*height), (int) (0.132*height)),
                    Dx = (int) (0.0905*height),
                    Dy = (int) (0.1565*height),
                    HeroPathPoints =
                        new[]
                        {
                            new Point(1, 1), new Point(1, (int) (0.0185*height)),
                            new Point(heroWidth, heroHeight),
                            new Point(heroWidth, heroHeight - (int) (0.0165*height))
                        },
                    HeroName1 = new Point(RoundUp(0.0138888888888889 * height), RoundUp(0.1722222222222222 * height))
                },
                Right = new SidePosition
                {
                    Ban1 = new Point((int) (width - 0.45*height), (int) (0.016*height)),
                    Ban2 =
                        new Point((int) (width - 0.45*height),
                            (int) (0.016*height) + (int) (0.023*height) + (int) (0.015*height)),
                    Pick1 = new Point((int) (width - 0.195*height), (int) (0.132*height)),
                    Dx = (int) (-0.0905*height),
                    Dy = (int) (0.1565*height),
                    HeroPathPoints =
                        new[]
                        {
                            new Point(heroWidth, 1), new Point(heroWidth, 1 + (int) (0.0185*height)),
                            new Point(1, heroHeight),
                            new Point(1, heroHeight - (int) (0.0165*height))
                        },
                    HeroName1 = new Point(RoundUp(width - 0.012037037037037 * height), RoundUp(0.175 * height))
                },
                MapPosition = new MapPosition
                {
                    Location = new Point((int) (width * 0.5 - 0.18 * height), 0),
                    Width = (int) (0.36*height),
                    Height = (int) (0.03563*height)
                },
                OverlapPoints = new OverlapPoints()
                {
                    AppearanceFramePoint = new Point(RoundUp(0.0666666666666667 * height), RoundUp(0.4231481481481481 * height)),
                    FrameRightBorderPoint = new Point(RoundUp(width - 0.012037037037037 * height), RoundUp(0.8685185185185185 * height)),
                    SkillFramePoint = new Point(RoundUp(0.0740740740740741 * height), RoundUp(0.7712962962962963 * height)),
                    TalentFramePoint = new Point(RoundUp(0.1435185185185185 * height), RoundUp(0.2953703703703704 * height)),
                    FullChatHorizontalPoint = new Point(RoundUp(width - 0.0185185185185185 * height), RoundUp(0.2296296296296296 * height)),
                    PartialChatlHorizontalPoint = new Point(RoundUp(width - 0.0185185185185185 * height), RoundUp(0.5851851851851852 * height))
                }
            };
            
            return position;
        }

        private static int RoundUp(double num)
        {
            return (int) (num + 0.5);
        }

        public void Exit()
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }
        }

        protected override void OnClose()
        {
            _hotKeyManager?.Dispose();
            _bpViewModel?.OcrUtil?.Dispose();
            AutoShowHideHelper = false;
            CfxRuntime.Shutdown();
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