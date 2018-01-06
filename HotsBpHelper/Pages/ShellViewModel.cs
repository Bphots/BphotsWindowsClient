using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Accord.Imaging;
using Accord.Imaging.Filters;
using GlobalHotKey;
using HotsBpHelper.Settings;
using HotsBpHelper.Utils;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Tasks;
using StatsFetcher;
using Stylet;
using Point = System.Drawing.Point;

namespace HotsBpHelper.Pages
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly IWebFileUpdaterViewModelFactory _webFileUpdaterViewModelFactory;

        private readonly IBpViewModelFactory _bpViewModelFactory;
        private readonly IMMRViewModelFactory _mmrViewModelFactory;

        private readonly IImageUtil _imageUtil;

        private readonly HotKeyManager _hotKeyManager;

        private BpViewModel _bpViewModel;
        private MMRViewModel _mmrViewModel;

        private bool _isLoaded;

        private readonly Form1 _form1 = new Form1();

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
                            CheckBpUi();
                        });
                    }
                }
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


        public ShellViewModel(IWebFileUpdaterViewModelFactory webFileUpdaterViewModelFactory, IBpViewModelFactory bpViewModelFactory, IMMRViewModelFactory mmrViewModelFactory, IImageUtil imageUtil)
        {
            _webFileUpdaterViewModelFactory = webFileUpdaterViewModelFactory;
            _bpViewModelFactory = bpViewModelFactory;
            _mmrViewModelFactory = mmrViewModelFactory;
            _imageUtil = imageUtil;
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
                // ���ǵ���ģ��,�������
                Update();
            }
            InitSettings();
            RegisterHotKey();
            if (!App.Debug && WindowManager.ShowDialog(_webFileUpdaterViewModelFactory.CreateViewModel()) != true)
            {
                Application.Current.Shutdown();
                return;
            }
            _bpViewModel = _bpViewModelFactory.CreateViewModel();
            WindowManager.ShowWindow(_bpViewModel);
            _mmrViewModel = _mmrViewModelFactory.CreateViewModel();
            WindowManager.ShowWindow(_mmrViewModel);
            _form1.ShowBallowNotify(L("Started"), L("StartedTips"));
            //form1.kill();
            AutoShowHideHelper = true; // Ĭ�������Զ�����
            AutoShowMmr = true; // Ĭ�������Զ���ʾMMR
            _isLoaded = true;
            base.OnViewLoaded();
        }

        private void CheckBpUi()
        {
            var etm = new ExhaustiveTemplateMatching(0.9f);
            var grayLockImages = Directory.GetFiles(Path.Combine(App.AppPath, @"Images\lock"))
                .Select(file => new Bitmap(file))
                .Select(Grayscale.CommonAlgorithms.BT709.Apply)
                .ToArray();
            while (AutoShowHideHelper)
            {
                bool foundBpUi = false;
                IntPtr hwnd = Win32.GetForegroundWindow();
                Int32 pid = Win32.GetWindowProcessID(hwnd);
                Process p = Process.GetProcessById(pid);
                bool inHotsGame = p.ProcessName.StartsWith(Const.HEROES_PROCESS_NAME);
                if (inHotsGame || p.ProcessName.StartsWith(Const.HOTSBPHELPER_PROCESS_NAME) || App.NotCheckProcess)
                {
                    using (var topScreenImage = _imageUtil.CaptureScreen(0, 0, App.AppSetting.MyPosition.Width, App.AppSetting.MyPosition.Height / 4))
                    {
                        using (var grayScreen = Grayscale.CommonAlgorithms.BT709.Apply(topScreenImage))
                        {
                            foreach (var grayLockImage in grayLockImages)
                            {
                                var tm = etm.ProcessImage(grayScreen, grayLockImage);
                                if (tm.Length > 1)
                                {
                                    foundBpUi = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                bool helperShowed = _bpViewModel.View?.Visibility == Visibility.Visible;
                //                Logger.Trace("process: {0}, foundBpUi: {1}, showed: {2}", p.ProcessName, foundBpUi, helperShowed);
                if (foundBpUi && !helperShowed || !foundBpUi && helperShowed)
                {
                    //ToggleBpVisible(inHotsGame && !foundBpUi);
                    ToggleBpVisible(false);
                }
                Thread.Sleep(1500);
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
                    });
                }
                Thread.Sleep(1000);

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
                ManuallyShowHideHelper();
            }
            else if (e.HotKey.Key == Key.C)
            {
                string captureName = Path.Combine(App.AppPath, "Screenshots", DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".bmp");
                _imageUtil.CaptureScreen().Save(captureName);
            }
        }

        public void ManuallyShowHideHelper()
        {
            AutoShowHideHelper = false;
            ToggleBpVisible(true);
        }

        private void ToggleBpVisible(bool clear)
        {
            if (!_isLoaded)
            {
                return;
            }
            Execute.OnUIThread(() =>
            {
                if (clear)
                {
                    _bpViewModel.Init();
                    _bpViewModel.Reload();
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

                _form1.ShowBallowNotify();
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
                App.AppSetting = Its.Configuration.Settings.Get<AppSetting>();
                var screenSize = ScreenUtil.GetScreenResolution();
                App.AppSetting.MyPosition = CaculatePosition((int) screenSize.Width, (int) screenSize.Height);
            }
            catch (Exception e)
            {
                ErrorView _errorView = new ErrorView(e.Message);
                _errorView.ShowDialog();
                _errorView.Pause();
            }
        }

        /// <summary>
        /// ���ݷֱ��ʶ�̬�������λ�úͳߴ�
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
                    HeroName1 = new Point((int) (0.013195*height), (int) (0.172222*height))
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
                    HeroName1 = new Point((int) (width - 0.011195*height), (int) (0.172222*height))
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

        public interface IMMRViewModelFactory
        {
            MMRViewModel CreateViewModel();
        }
    }
}