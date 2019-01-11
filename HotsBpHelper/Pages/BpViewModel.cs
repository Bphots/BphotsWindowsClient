using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using DotNetHelper;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Api.Security;
using HotsBpHelper.Factories;
using HotsBpHelper.HeroFinder;
using HotsBpHelper.Messages;
using HotsBpHelper.Services;
using HotsBpHelper.Settings;
using HotsBpHelper.Uploader;
using HotsBpHelper.UserControls;
using HotsBpHelper.Utils;
using ImageProcessor.HashProcessing;
using ImageProcessor.ImageProcessing;
using ImageProcessor.Ocr;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Stylet;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;

namespace HotsBpHelper.Pages
{
    public class BpViewModel : ViewModelBase, IHandle<ItemSelectedMessage>, IHandle<SideSelectedMessage>, IHandle<MapSelectedMessage>
    {
        private static readonly object LookForBpTokenLock = new object();

        private readonly List<HeroSelectorViewModel> _cachedHeroSelectorViewModels = new List<HeroSelectorViewModel>();
        private readonly IEventAggregator _eventAggregator;

        private static readonly List<int> BanSteps = new List<int> { 0, 1, 2, 8, 9, 10 };

        private readonly Dictionary<int, string> _lastIds = new Dictionary<int, string>()
        {
            {0, "0"},
            {1, "0"},
            {2, "0"},
            {8, "0"},
            {9, "0"},
            {10, "0"},
        };

        private readonly ViewModelFactory _viewModelFactory;

        private readonly ISecurityProvider _securityProvider;
        private bool _autoShowHideHelper;
        private bool _bpScreenLoaded;
        private bool _bpStarted;
        
        private bool _hasLookedForMap;
        private int _height;
        private HeroSelectorWindowViewModel _heroSelectorWindowViewModel;

        private bool _isAutoMode;

        private bool _isFirstAndSecondBanProcessing;

        private bool _isThirdAndFourthBanProcessing;

        private IList<IList<int>> _listBpSteps;

        private IList<Point> _listPositions;

        private IList<Point> _teamLeagueListPositions;

        private MapSelectorViewModel _mapSelectorViewModel;

        public ConcurrentDictionary<int, bool> ProcessingThreads { get; set; } = new ConcurrentDictionary<int, bool>();

        private CancellationTokenSource _scanningCancellationToken;
        
        private int _width;
        private readonly IToastService _toastService;
        private bool _showDevTool;
        private IRestApi _restApi;

        public bool OcrAvailable { get; set; }

        public BpViewModel(ViewModelFactory viewModelFactory, IEventAggregator eventAggregator, ISecurityProvider securityProvider, IRestApi restApi, IToastService toastService)
        {
            _viewModelFactory = viewModelFactory;
            _restApi = restApi;
            _eventAggregator = eventAggregator;
            _toastService = toastService;
            _securityProvider = securityProvider;
            _eventAggregator.Subscribe(this);
            _scanningCancellationToken = new CancellationTokenSource();
            
            try
            {
                OcrUtil = new OcrUtil();
                OcrUtil.Initialize();
                OcrAvailable = true;
            }
            catch (Exception)
            {
                // ignored
            }

            var unitPos = App.MyPosition.BpHelperPosition.ToUnitPoint();
            Left = unitPos.X;
            Top = unitPos.Y;

            _heroSelectorWindowViewModel = _viewModelFactory.CreateViewModel<HeroSelectorWindowViewModel>();

            var filePath = @Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "index.html#") + App.Language;
            LocalFileUri = filePath;

            WebCallbackListener.PresetRequested += WebCallbackListenerOnPresetRequested;
        }

        private void WebCallbackListenerOnPresetRequested(object sender, EventArgs e)
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "preset",
                Args = new[] { JsonConvert.SerializeObject(App.AdviceHeroInfos, serializerSettings), JsonConvert.SerializeObject(App.AdviceMapInfos, serializerSettings) }
            }, "BpChanel");
        }

        public BindableCollection<HeroSelectorViewModel> HeroSelectorViewModels =>
            _heroSelectorWindowViewModel?.HeroSelectorViewModels;

        internal void ReInitializeOcr()
        {
            try
            {
                OcrUtil.Initialize();
                OcrAvailable = true;
            }
            catch (Exception)
            {
                // ignored
            }
        }
        
        public int Left { get; set; }

        public int Top { get; set; }

        public int Width
        {
            get { return _width; }
            set { SetAndNotify(ref _width, value); }
        }

        public int Height
        {
            get { return _height; }
            set { SetAndNotify(ref _height, value); }
        }

        public string LocalFileUri { get; set; }

        public bool ShowDevTool { get { return _showDevTool; } set { SetAndNotify(ref _showDevTool, value); } }

        public BpStatus BpStatus { get; set; }

        public OcrUtil OcrUtil { get; set; }

        public bool AutoShowHideHelper
        {
            get { return _autoShowHideHelper; }
            set
            {
                if (_autoShowHideHelper == value)
                    return;

                _autoShowHideHelper = value;
                if (value)
                {
                    if (!BpScreenLoaded)
                        Task.Run(LookForBpScreen).ConfigureAwait(false);
                }
                else
                {
                    MapOnlyCancellationToken?.Cancel();
                }
            }
        }

        public bool IsAutoMode
        {
            get { return _isAutoMode; }
            set
            {
                if (_isAutoMode == value)
                    return;

                _isAutoMode = value;
                if (_mapSelectorViewModel != null)
                    _mapSelectorViewModel.ButtonVisibility = !IsAutoMode ? Visibility.Visible : Visibility.Hidden;

                OnRemindUserDetectMode();

                if (!BpStarted && !BpScreenLoaded)
                {
                    _scanningCancellationToken?.Cancel();
                    return;
                }

                if (!BpStarted && BpScreenLoaded)
                {
                    if (value && !_hasLookedForMap)
                        Task.Run(LookForMap).ConfigureAwait(false);
                    else
                        _scanningCancellationToken?.Cancel();
                    return;
                }

                if (value)
                {
                    // switch off all pending HeroSelectors
                    //foreach (var i in _listBpSteps[BpStatus.CurrentStep])
                    //{
                    //    if (BpStatus.CurrentStep == 0 || BpStatus.CurrentStep == 1 || BpStatus.CurrentStep == 5 ||
                    //        BpStatus.CurrentStep == 6) continue;

                    //    var vm = HeroSelectorViewModels.First(v => v.Id == i);
                    //    if (!vm.Selected)
                    //        vm.InteractionVisible = false;
                    //}

                    MapOnlyCancellationToken?.Cancel();
                    _scanningCancellationToken = new CancellationTokenSource();
                }
                else
                {
                    _scanningCancellationToken?.Cancel();
                }

                ProcessStep();
            }
        }

        private CancellationTokenSource MapOnlyCancellationToken { get; set; }

        public bool BpStarted
        {
            get { return _bpStarted; }
            set
            {
                SetAndNotify(ref _bpStarted, value);
            }
        }

        public bool BpScreenLoaded
        {
            get { return _bpScreenLoaded; }
            set
            {
                if (_bpScreenLoaded == value)
                    return;

                Manager.InBpSuspend = value;
                Execute.OnUIThread(() =>
                {
                    _bpScreenLoaded = value;
                    if (_bpScreenLoaded)
                        OnRemindBpStart();
                });
            }
        }

        public void Handle(ItemSelectedMessage message)
        {
            try
            {
                if (message.ItemInfo == null)
                {
                    if (IsAutoMode || BpStatus.CurrentStep <= 0)
                        return;

                    if (BpStatus.StepSelectedIndex.Any())
                    {
                        foreach (var i in _listBpSteps[BpStatus.CurrentStep])
                        {
                            var vm = HeroSelectorViewModels.First(v => v.Id == i);
                            vm.Selected = false;
                            vm.SelectedItemInfo = null;
                        }
                        BpStatus.StepSelectedIndex.Clear();
                    }
                    else
                    {
                        foreach (var i in _listBpSteps[BpStatus.CurrentStep])
                        {
                            var vm = HeroSelectorViewModels.First(v => v.Id == i);
                            vm.InteractionVisible = false;
                        }

                        BpStatus.CurrentStep--;

                        {
                            var i = _listBpSteps[BpStatus.CurrentStep].Last();
                            var vm = HeroSelectorViewModels.First(v => v.Id == i);
                            vm.Selected = false;
                            vm.SelectedItemInfo = null;

                            if (_listBpSteps[BpStatus.CurrentStep].Count > 1)
                            {
                                var vmSelected = HeroSelectorViewModels.First(v => v.Id != _listBpSteps[BpStatus.CurrentStep].First());
                                if (vmSelected != null)
                                    BpStatus.StepSelectedIndex = new HashSet<int>() { vmSelected.Id };
                                else
                                    BpStatus.StepSelectedIndex.Clear();
                            }
                            else
                                BpStatus.StepSelectedIndex.Clear();
                        }
                    }
                    
                    InvokeScript("update", new List<Tuple<string, string>>
                    {
                        Tuple.Create("chose", string.Join("|",
                            HeroSelectorViewModels
                                .Where(vm => vm.SelectedItemInfo != null)
                                .Select(vm => vm.SelectedItemInfo.Id))),
                        Tuple.Create("map", BpStatus.Map),
                        Tuple.Create("lang", App.Language)
                    });
                    return;
                }

                var idList = new List<string>();

                foreach (var vm in HeroSelectorViewModels)
                {
                    if (vm.SelectedItemInfo != null)
                        idList.Add(vm.SelectedItemInfo.Id);
                }
            
                if (App.CustomConfigurationSettings.UploadBanSample && BanSteps.Contains(message.SelectorId) && !string.IsNullOrEmpty(message.ItemInfo?.Id) && message.ItemInfo.Id != "0")
                {
                    if (_lastIds[message.SelectorId] != message.ItemInfo?.Id)
                    {
                        Task.Run(() => UploadSampleAsync(message.ItemInfo.Id, BanSteps.IndexOf(message.SelectorId)));
                        _lastIds[message.SelectorId] = message.ItemInfo?.Id;
                    }
                }

                InvokeScript("update", new List<Tuple<string, string>>
                {
                    Tuple.Create("chose", string.Join("|", idList)),
                    Tuple.Create("map", BpStatus.Map),
                    Tuple.Create("lang", App.Language)
                });
                if (BpStatus.StepSelectedIndex.Contains(message.SelectorId) || // 修改本轮选过的英雄
                    !_listBpSteps[BpStatus.CurrentStep].Contains(message.SelectorId)) // 修改其他轮选过的英雄
                {
                    // 修改英雄选择,无需处理
                }
                else
                {
                    // 新英雄选择,判断本轮是否已选够英雄
                    BpStatus.StepSelectedIndex.Add(message.SelectorId);
                    if (BpStatus.StepSelectedIndex.Count == _listBpSteps[BpStatus.CurrentStep].Count)
                    {
                        // 选够了,下一步
                        if (BpStatus.CurrentStep < 11)
                        {
                            BpStatus.CurrentStep++;
                            ProcessStep();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // TODO Ignore for test, please remove the catch
            }
        }

        private async Task UploadSampleAsync(string id, int index)
        {
            try
            {
                var path = Path.Combine(App.AppPath, "Images\\Heroes");
                var path2 = string.Format("{0}x{1}", App.AppSetting.Position.Width, App.AppSetting.Position.Height);
                FilePath finalPath = Path.Combine(path, path2, string.Format("{0}_{1:yyyyMMddhhmmss}.jpg", id, DateTime.Now));
                if (!finalPath.GetDirPath().Exists)
                    Directory.CreateDirectory(finalPath.GetDirPath());

                var imageUtils = new ImageUtils();
                lock (ImageProcessingHelper.GDILock)
                {
                    using (var bmp = imageUtils.CaptureBanArea(App.AppSetting.Position.BanPositions[index]))
                    {
                        bmp.Save(finalPath);
                    }
                }

                await _restApi.UploadImage(finalPath, id).ConfigureAwait(false);
                finalPath.DeleteIfExists();
            }
            catch
            {
                // ignored
            }
        }

        public void Handle(SideSelectedMessage message)
        {
            //收到重置命令，重置
            if (message.ItemInfo == null)
            {
                Init();
                InitializeAllHeroSelector();
                Show();
                return;
            }
            BpStarted = true;
            OcrUtil?.ClearProcessedPositions();
            // 初始化BP过程
            BpStatus = new BpStatus
            {
                Map = message.ItemInfo.Id,
                FirstSide = message.Side
            };

            var side = (int) BpStatus.FirstSide;
            InvokeScript("init", side.ToString(), App.CustomConfigurationSettings.LanguageForBphots, App.CustomConfigurationSettings.LanguageForMessage, App.CustomConfigurationSettings.LanguageForGameClient);
            InvokeScript("update", new List<Tuple<string, string>>
            {
                Tuple.Create("chose", ""),
                Tuple.Create("map", BpStatus.Map),
                Tuple.Create("lang", App.Language)
            });

            // Position下标位置示意
            //   0 1 2   8 9 10
            // 3                11
            // 4                12
            // 5                13
            // 6                14
            // 7                15

            if (message.Side == BpStatus.Side.Left)
            {
                _listBpSteps = new List<IList<int>> // BP总共12手
                {
                    new List<int> {0},
                    new List<int> {8},
                    new List<int> {1},
                    new List<int> {9},
                    new List<int> {3},
                    new List<int> {11, 12}, // 倒序是为了让9获得输入焦点
                    new List<int> {4, 5},
                    new List<int> {10},
                    new List<int> {2},
                    new List<int> {13, 14},
                    new List<int> {6, 7},
                    new List<int> {15}
                };
            }
            else
            {
                _listBpSteps = new List<IList<int>> // BP总共12手
                {
                    new List<int> {8},
                    new List<int> {0},
                    new List<int> {9},
                    new List<int> {1},
                    new List<int> {11},
                    new List<int> {3, 4},
                    new List<int> {12, 13},
                    new List<int> {2},
                    new List<int> {10},
                    new List<int> {5, 6},
                    new List<int> {14, 15},
                    new List<int> {7}
                };
            }

            BpStatus.CurrentStep = 0;
            ProcessStep();
        }

        public event EventHandler TurnOffAutoDetectMode;
        public event EventHandler RemindDetectMode;
        public event EventHandler RemindBpStart;

        private async Task DelayedResetAsync()
        {
            CancelAllActiveScan();
            await Task.Delay(15000);
            Execute.OnUIThread(() => Reset());
        }

        public void Reset(bool hide = true)
        {
            CancelAllActiveScan();
            Init();
            InitializeAllHeroSelector();
            Reload();
            if (hide)
                Hide();
            
            if (AutoShowHideHelper)
            {
                Task.Run(LookForBpScreen).ConfigureAwait(false);
            }
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            FillPositions();
            InitializeMapSelector();
            InitializeAllHeroSelector();
        }

        public void SelectMap(string map)
        {
            _mapSelectorViewModel.Select(map);
        }

        public void InitializeMapSelector()
        {
            _mapSelectorViewModel?.RequestClose();
            _mapSelectorViewModel = _viewModelFactory.CreateViewModel<MapSelectorViewModel>();
            _mapSelectorViewModel.ButtonVisibility = !IsAutoMode ? Visibility.Visible : Visibility.Hidden;
            _mapSelectorViewModel.Id = 0;
            _mapSelectorViewModel.SetCenterAndTop(App.MyPosition.MapSelectorPosition);

            _mapSelectorViewModel.Visibility = Visibility.Hidden;
            WindowManager.ShowWindow(_mapSelectorViewModel);
            ((Window) _mapSelectorViewModel.View).Owner = (Window) View;
            _mapSelectorViewModel.View.Visibility = Visibility.Hidden;
            /*
                        _eventAggregator.Publish(new ShowWindowMessage
                        {
                            ViewModel = vm,
                        });
            */
        }

        public void InitializeAllHeroSelector()
        {
            // unspoiled cache, no need to re-initialize
            if (!HeroSelectorViewModels.Any() && _cachedHeroSelectorViewModels.Any())
                return;

            if (_cachedHeroSelectorViewModels.Any())
                CloseHeroSelectorWindows();

            _heroSelectorWindowViewModel = _viewModelFactory.CreateViewModel<HeroSelectorWindowViewModel>();
            WindowManager.ShowWindow(_heroSelectorWindowViewModel);
            ((Window) _heroSelectorWindowViewModel.View).Owner = (Window) View;

            for (var i = 0; i <= 15; ++i)
            {
                var vm = _viewModelFactory.CreateViewModel<HeroSelectorViewModel>();
                _cachedHeroSelectorViewModels.Add(vm);
                vm.Id = i;
                vm.InitializeUnselect();
                var position = _listPositions[i];
                if (i < 8)
                {
                    vm.SetLeftAndTop(position);
                }
                else
                {
                    vm.SetRightAndTop(position);
                }
                vm.InteractionVisible = false;
                vm.LayerVisible = false;
                vm.Refresh();
            }
        }

        public void PopulateCachedHeroSelectorWindows()
        {
            if (!_cachedHeroSelectorViewModels.Any())
                InitializeAllHeroSelector();

            foreach (var pointIndex in _listBpSteps.SelectMany(c => c))
            {
                var vm = _cachedHeroSelectorViewModels.First(v => v.Id == pointIndex);
                HeroSelectorViewModels.Add(vm);
            }
        }

        public void ShowHeroSelector(int pointIndex, string name = null)
        {
            if (!HeroSelectorViewModels.Any())
                PopulateCachedHeroSelectorWindows();

            if ((_listBpSteps[4].Contains(pointIndex) || _listBpSteps[9].Contains(pointIndex)) && name != null)
                PopulateBanSelector(pointIndex);

            var vm = HeroSelectorViewModels.First(v => v.Id == pointIndex);
            vm.InteractionVisible = true;
            if (name != null)
            {
                var heroInUi =
                    vm.ItemsInfos.First(
                        m => m.Id == App.OcrHeroInfos.First(om => om.Name == name).Id.ToString()).Name;
                vm.Select(heroInUi);
                vm.ConfirmSelection();
            }
        }

        public void ForceBanProcess(int banSteps)
        {
            var firstBanId = _listBpSteps[banSteps - 1].First();
            var secondBanId = _listBpSteps[banSteps].First();
            if (!HeroSelectorViewModels.First(v => v.Id == secondBanId).InteractionVisible)
            {
                if (HeroSelectorViewModels.First(v => v.Id == firstBanId).SelectedItemInfo == null)
                {
                    HeroSelectorViewModels.First(v => v.Id == firstBanId).Select(L("NO_CHOOSE"));
                    HeroSelectorViewModels.First(v => v.Id == firstBanId).ConfirmSelection();
                }
            }
        }
        
        private void ForceFourthPickProcess()
        {
            var secondBanId = _listBpSteps[8].First();
            if (BpStatus.CurrentStep == 8)
            {
                if (HeroSelectorViewModels.First(v => v.Id == secondBanId).SelectedItemInfo == null)
                {
                    HeroSelectorViewModels.First(v => v.Id == secondBanId).Select(L("NO_CHOOSE"));
                    HeroSelectorViewModels.First(v => v.Id == secondBanId).ConfirmSelection();
                }
            }
        }

        public void ForceFirstPickProcess()
        {
            var secondBanId = _listBpSteps[3].First();
            if (BpStatus.CurrentStep == 3)
            {
                if (HeroSelectorViewModels.First(v => v.Id == secondBanId).SelectedItemInfo == null)
                {
                    HeroSelectorViewModels.First(v => v.Id == secondBanId).Select(L("NO_CHOOSE"));
                    HeroSelectorViewModels.First(v => v.Id == secondBanId).ConfirmSelection();
                }
            }
        }

        private void PopulateBanSelector(int pointIndex)
        {
            var firstBanId = _listBpSteps[4].Contains(pointIndex) ? _listBpSteps[0].First() : _listBpSteps[5].First();
            var secondBanId = _listBpSteps[4].Contains(pointIndex) ? _listBpSteps[1].First() : _listBpSteps[6].First();

            if (!HeroSelectorViewModels.First(v => v.Id == firstBanId).InteractionVisible)
            {
                var vm = HeroSelectorViewModels.First(v => v.Id == firstBanId);
                vm.InteractionVisible = true;
                vm.Select(L("NO_CHOOSE"));
            }
            else if (HeroSelectorViewModels.First(v => v.Id == firstBanId).SelectedItemInfo == null)
            {
                HeroSelectorViewModels.First(v => v.Id == firstBanId).Select(L("NO_CHOOSE"));
            }
            if (!HeroSelectorViewModels.First(v => v.Id == secondBanId).InteractionVisible)
            {
                var vm = HeroSelectorViewModels.First(v => v.Id == secondBanId);
                vm.InteractionVisible = true;
                vm.Select(L("NO_CHOOSE"));
            }
            else if (HeroSelectorViewModels.First(v => v.Id == secondBanId).SelectedItemInfo == null)
            {
                HeroSelectorViewModels.First(v => v.Id == secondBanId).Select(L("NO_CHOOSE"));
            }
            if (_listBpSteps[4].Contains(pointIndex))
            {
                var thirdBanId = _listBpSteps[2].First();
                var fourthBanId = _listBpSteps[3].First();
                if (!HeroSelectorViewModels.First(v => v.Id == thirdBanId).InteractionVisible)
                {
                    var vm = HeroSelectorViewModels.First(v => v.Id == thirdBanId);
                    vm.InteractionVisible = true;
                    vm.Select(L("NO_CHOOSE"));
                }
                else if (HeroSelectorViewModels.First(v => v.Id == thirdBanId).SelectedItemInfo == null)
                {
                    HeroSelectorViewModels.First(v => v.Id == thirdBanId).Select(L("NO_CHOOSE"));
                }
                if (!HeroSelectorViewModels.First(v => v.Id == fourthBanId).InteractionVisible)
                {
                    var vm = HeroSelectorViewModels.First(v => v.Id == fourthBanId);
                    vm.InteractionVisible = true;
                    vm.Select(L("NO_CHOOSE"));
                }
                else if (HeroSelectorViewModels.First(v => v.Id == fourthBanId).SelectedItemInfo == null)
                {
                    HeroSelectorViewModels.First(v => v.Id == fourthBanId).Select(L("NO_CHOOSE"));
                }
            }


            BpStatus.CurrentStep = _listBpSteps[4].Contains(pointIndex) ? 4 : 9;
        }


        private void FillPositions()
        {
            SidePosition sidePosition;
            int x, y;
            int dx, dy;
            int teamLeagueX, teamLeaguey;
            int teamLeaguedy;
            _listPositions = new List<Point>(16); // BP总共16个选择
            _teamLeagueListPositions = new List<Point>(16); // BP总共16个选择

            teamLeaguedy = App.AppSetting.Position.TeamLeagueDy;

            // Left
            sidePosition = App.MyPosition.Left;
            _listPositions.Add(sidePosition.Ban1);
            _listPositions.Add(sidePosition.Ban2);
            _listPositions.Add(sidePosition.Ban3);
            _teamLeagueListPositions.Add(sidePosition.Ban1);
            _teamLeagueListPositions.Add(sidePosition.Ban2);
            _teamLeagueListPositions.Add(sidePosition.Ban3);
            x = sidePosition.Pick1.X;
            y = sidePosition.Pick1.Y;
            dx = sidePosition.Dx;
            dy = sidePosition.Dy;

            teamLeagueX = (int) (sidePosition.Pick1.X * 1.05 + dx);
            teamLeaguey = sidePosition.Pick1.Y;
            for (var i = 0; i < 5; i++)
            {
                _listPositions.Add(new Point(x, y));
                _teamLeagueListPositions.Add(new Point(teamLeagueX, teamLeaguey));
                x += dx;
                y += dy;
                dx = -dx;
                teamLeaguey += teamLeaguedy;
            }
            // Right
            sidePosition = App.MyPosition.Right;
            _listPositions.Add(sidePosition.Ban1);
            _listPositions.Add(sidePosition.Ban2);
            _listPositions.Add(sidePosition.Ban3);
            _teamLeagueListPositions.Add(sidePosition.Ban1);
            _teamLeagueListPositions.Add(sidePosition.Ban2);
            _teamLeagueListPositions.Add(sidePosition.Ban3);
            x = sidePosition.Pick1.X;
            y = sidePosition.Pick1.Y;
            dx = sidePosition.Dx;
            dy = sidePosition.Dy;
            
            teamLeagueX = (int)(sidePosition.Pick1.X * 0.95);
            teamLeaguey = sidePosition.Pick1.Y;

            for (var i = 0; i < 5; i++)
            {
                _listPositions.Add(new Point(x, y));
                _teamLeagueListPositions.Add(new Point(teamLeagueX, teamLeaguey));
                x += dx;
                y += dy;
                dx = -dx;
                teamLeaguey += teamLeaguedy;
            }
        }

        private void CloseHeroSelectorWindows()
        {
            HeroSelectorViewModels?.Clear();
            _cachedHeroSelectorViewModels.Clear();
            _heroSelectorWindowViewModel.RequestClose();
        }

        private void InvokeScript(string scriptName, params string[] args)
        {
            _eventAggregator.Publish(new InvokeScriptMessage
            {
                ScriptName = scriptName,
                Args = args
            });
        }

        private void InvokeScript(string scriptName, IList<Tuple<string, string>> parameters)
        {
            var sp = _securityProvider.CaculateSecurityParameter(parameters);
            parameters.Add(Tuple.Create("timestamp", sp.Timestamp));
            parameters.Add(Tuple.Create("client_patch", sp.Patch));
            parameters.Add(Tuple.Create("nonce", sp.Nonce));
            parameters.Add(Tuple.Create("sign", sp.Sign));
            _eventAggregator.Publish(new InvokeScriptMessage
            {
                ScriptName = scriptName,
                Args = parameters.Select(tuple => tuple.Item2).ToArray()
            }, "BpChanel");
        }

        protected override void OnClose()
        {
            _mapSelectorViewModel.RequestClose();
            CloseHeroSelectorWindows();
            OcrUtil?.Dispose();
            base.OnClose();
        }

        public void Init()
        {
            _lastIds[0] = "0";
            _lastIds[1] = "0";
            _lastIds[2] = "0";
            _lastIds[8] = "0";
            _lastIds[9] = "0";
            _lastIds[10] = "0";

            InvokeScript("init", "0", App.CustomConfigurationSettings.LanguageForBphots, App.CustomConfigurationSettings.LanguageForMessage, App.CustomConfigurationSettings.LanguageForGameClient);
        }

        public void Reload()
        {
            //重启界面
            _mapSelectorViewModel?.RequestClose();
            // CloseHeroSelectorWindows();
            SwitchAdviceWindow(false);
            BpStarted = false;
            BpScreenLoaded = false;
            BpStatus = null;
            _hasLookedForMap = false;
            _isFirstAndSecondBanProcessing = false;
            _isThirdAndFourthBanProcessing = false;
            InitializeMapSelector();
            OcrUtil?.ClearProcessedPositions();
        }

        public void CancelAllActiveScan()
        {
            _scanningCancellationToken?.Cancel();
            MapOnlyCancellationToken?.Cancel();
        }

        private async Task OcrAsync(IEnumerable<int> steps, CancellationToken cancellationToken)
        {
            if (BpStatus.CurrentStep == 4)
            {
                bool isTeamLeague = await OcrUtil.CheckIfInTeamMatchAsync(BpStatus.FirstSide == BpStatus.Side.Left ? OcrUtil.ScanSide.Left : OcrUtil.ScanSide.Right, cancellationToken);

                if (isTeamLeague)
                {
                    Execute.OnUIThread(RelocateSelectors);
                }
                
                Execute.OnUIThread(() =>
                {
                    foreach (var i in _listBpSteps[4])
                    {
                        ShowHeroSelector(i);
                    }
                });
            }

            var stepToProcess = new List<int>();
            foreach (var i in steps)
            {
                if (ProcessingThreads.ContainsKey(i) && ProcessingThreads[i])
                    continue;

                if (HeroSelectorViewModels.Any(v => v.Id == i && v.Selected))
                    continue;

                stepToProcess.Add(i);
                ProcessingThreads[i] = true;
            }
            if (!stepToProcess.Any())
                return;

            try
            {
                if (stepToProcess[0] <= 8 && OcrUtil.IsInitialized)
                    await OcrUtil.ScanLabelAsync(stepToProcess, this, OcrUtil.ScanSide.Left, cancellationToken).ConfigureAwait(false);
                else
                    await OcrUtil.ScanLabelAsync(stepToProcess, this, OcrUtil.ScanSide.Right, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                Logger.Trace("OcrAsync failed first attempt on {0}", string.Join(",", stepToProcess));
                try
                {
                    foreach (var i in stepToProcess)
                    {
                        ProcessingThreads[i] = false;
                    }

                    if (stepToProcess[0] <= 8 && OcrUtil.IsInitialized)
                        await OcrUtil.ScanLabelAsync(stepToProcess, this, OcrUtil.ScanSide.Left, cancellationToken).ConfigureAwait(false);
                    else
                        await OcrUtil.ScanLabelAsync(stepToProcess, this, OcrUtil.ScanSide.Right, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    Logger.Trace("OcrAsync failed 2nd attempt on {0}", string.Join(",", stepToProcess));
                    Execute.OnUIThread(() => { Reset(); });
                }
            }
            finally
            {
                foreach (var i in stepToProcess)
                {
                    ProcessingThreads[i] = false;
                }
            }
        }

        private void RelocateSelectors()
        {
            if (_cachedHeroSelectorViewModels.Count < 16)
                return;

            for (var i = 0; i <= 15; ++i)
            {
                var vm = _cachedHeroSelectorViewModels[i];
                var position = _teamLeagueListPositions[i];
                if (i < 8 && i > 2)
                {
                    vm.SetLeftAndTop(position);
                }
                else if (i > 10)
                {
                    vm.SetRightAndTop(position);
                }
                vm.Refresh();
            }
        }


        private void ProcessStep()
        {
            if (_listBpSteps == null || BpStatus.CurrentStep == _listBpSteps.Count) return;

            // 显示英雄选择框
            if (IsAutoMode)
                ProcessAutoStep();
            else
                ProcessManualStep();
        }

        private void ProcessAutoStep()
        {
            if (BpStatus.CurrentStep != 4)
                foreach (var i in _listBpSteps[BpStatus.CurrentStep])
                {
                    ShowHeroSelector(i);
                }

            if (!(BpStatus.CurrentStep == 0 || BpStatus.CurrentStep == 1 || 
                BpStatus.CurrentStep == 2 || BpStatus.CurrentStep == 3 || 
                BpStatus.CurrentStep == 7 || BpStatus.CurrentStep == 8))
            {
                Task.Run(() => OcrAsync(_listBpSteps[BpStatus.CurrentStep], _scanningCancellationToken.Token))
                    .ConfigureAwait(false);
                return;
            }
            
            if ((BpStatus.CurrentStep == 0 || BpStatus.CurrentStep == 1 || BpStatus.CurrentStep == 2 || BpStatus.CurrentStep == 3) && !_isFirstAndSecondBanProcessing)
            {
                Task.Run(ProcessFirstToFourthBan).ConfigureAwait(false);
                return;
            }
            if ((BpStatus.CurrentStep == 7 || BpStatus.CurrentStep == 8) && !_isThirdAndFourthBanProcessing)
            {
                Task.Run(ProcessFifthAndSixthBan).ConfigureAwait(false);
            }
        }

        private void ProcessManualStep()
        {
            var hasFocused = false;
            foreach (var i in _listBpSteps[BpStatus.CurrentStep])
            {
                ShowHeroSelector(i);
            }
        }

        private async Task ProcessFifthAndSixthBan()
        {
            _isThirdAndFourthBanProcessing = true;
            var finder = new Finder();
            var stageInfo = new StageInfo();
            await AwaitStageAsync(stageInfo, finder, 8);

            if (!_scanningCancellationToken.IsCancellationRequested)
                Execute.OnUIThread(() => ForceBanProcess(8));

            await AwaitStageAsync(stageInfo, finder, 9);
            if (!_scanningCancellationToken.IsCancellationRequested)
                Execute.OnUIThread(ForceFourthPickProcess);

            _isThirdAndFourthBanProcessing = false;
        }

        private async Task AwaitStageAsync(StageInfo stageInfo, Finder finder, int stage)
        {
            try
            {
                OcrAsyncChecker.CheckThread(OcrAsyncChecker.AwaitStagAsyncChecker);
                bool warned = false;
                int inBpFail = 0;
                while (stageInfo.Step < stage && stage > BpStatus.CurrentStep && !_scanningCancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(500);
                    if (OcrUtil.SuspendScanning)
                        continue;
                    
                    stageInfo = finder.GetStageInfo();

                    if (stageInfo.Step != -1 || warned)
                    {
                        inBpFail = 0;
                        continue;
                    }

                    inBpFail++;
                    if (inBpFail != 10)
                        continue;

                    warned = true;
                    WarnNotInBp();
                }

                try
                {
                    var selectorId = _listBpSteps[stage - 1].First();
                    var idTuple = finder.GetBanHero(BanSteps.IndexOf(selectorId));
                    if (idTuple.Item1 > 0 && idTuple.Item1 < 1000)
                    {
                        if (idTuple.Item2 < 20)
                        {
                            Execute.OnUIThread(() => { ShowHeroSelector(selectorId, App.OcrHeroInfos.First(h => h.Id == idTuple.Item1).Name); });
                            return;
                        }

                        await Task.Delay(500);
                        var idConfirm = finder.GetBanHero(BanSteps.IndexOf(selectorId));
                        if (idConfirm.Item1 == idTuple.Item1)
                            Execute.OnUIThread(() => { ShowHeroSelector(selectorId, App.OcrHeroInfos.First(h => h.Id == idTuple.Item1).Name); });
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            finally 
            {
                OcrAsyncChecker.CleanThread(OcrAsyncChecker.AwaitStagAsyncChecker);
            }
            
        }

        private async Task LookForBpScreen()
        {
            _scanningCancellationToken?.Cancel();
            // cancel the other thread
            MapOnlyCancellationToken?.Cancel();
            CancellationTokenSource currentCancellationToken;
            lock (LookForBpTokenLock)
            {
                MapOnlyCancellationToken = new CancellationTokenSource();
                currentCancellationToken = MapOnlyCancellationToken;
            }
            await OcrUtil.LookForBpScreen(MapOnlyCancellationToken.Token);
            if (!currentCancellationToken.IsCancellationRequested)
                Execute.OnUIThread(() =>
                {
                    LoadMapUi();
                    if (!IsAutoMode)
                        BpScreenLoaded = true;
                });
            else
                return;

            if (IsAutoMode)
            {
                _hasLookedForMap = true;
                if (currentCancellationToken.IsCancellationRequested)
                    return;

                await LookForMap().ConfigureAwait(false);
            }
        }

        private async Task LookForMap()
        {
            _hasLookedForMap = true;
            try
            {
                _scanningCancellationToken = new CancellationTokenSource();
                var map = await OcrUtil.LookForMap(_scanningCancellationToken.Token).ConfigureAwait(false);
                // false positive
                if (string.IsNullOrEmpty(map))
                {
                    Execute.OnUIThread(() => Reset());
                    return;
                }

                if (_scanningCancellationToken.IsCancellationRequested)
                    return;

                if (!IsAutoMode)
                    Execute.OnUIThread(() => { BpScreenLoaded = true; });

                Execute.OnUIThread(() => SwitchMapSelector(true, false));
                if (!string.IsNullOrEmpty(map))
                {
                    var mapInUi =
                        _mapSelectorViewModel.ItemsInfos.First(
                            m => m.Id == App.OcrMapInfos.First(om => om.Name == map).Code).Name;

                    Execute.OnUIThread(() => SelectMap(mapInUi));
                }

                await ScanBpAsync();
            }
            finally
            {
                _hasLookedForMap = false;
            }
        }

        private async Task<StageInfo> ScanFirstPick()
        {
            var finder = new Finder();
            var stageInfo = new StageInfo {Step = -1};
            while (stageInfo.Step == -1 && !_scanningCancellationToken.IsCancellationRequested)
            {
                if (OcrUtil.SuspendScanning)
                {
                    await Task.Delay(1000);
                    continue;
                }
                stageInfo = finder.GetStageInfo();
                if (stageInfo.Step > -1)
                    return stageInfo;

                await Task.Delay(1000);
            }

            return stageInfo;
        }

        private async Task ScanBpAsync()
        {
            var logUtilScanBpAsync = new LogUtil(@".\logScanBpAsync.txt");

            logUtilScanBpAsync.Log("CheckFirstPick");
            var stageInfo = await ScanFirstPick().ConfigureAwait(false);

            if (_scanningCancellationToken.IsCancellationRequested)
                return;

            var isFirstPick = stageInfo.IsFirstPick;

            Execute.OnUIThread(() =>
            {
                var logUtil = new LogUtil(@".\logSelectSide.txt");
                logUtil.Log("SelectSide");
                SelectSide(isFirstPick ? BpStatus.Side.Left : BpStatus.Side.Right);
                logUtil.Flush();
            });
            logUtilScanBpAsync.Log("FirstPickChecked");
            logUtilScanBpAsync.Flush();

            //if (isFirstPick)
            //{
            //    await Task.Delay(6000);
            //    OcrUtil.AdjustPlaceHolderPosition();
            //}
        }

        private void LoadMapUi()
        {
            Init();
            Reload();
            if (!IsAutoMode)
                SwitchMapSelector(true);
            Show();
        }

        private async Task ProcessFirstToFourthBan()
        {
            _isFirstAndSecondBanProcessing = true;
            var finder = new Finder();
            var stageInfo = new StageInfo {Step = -1};
            await AwaitStageAsync(stageInfo, finder, 1);

            if (!_scanningCancellationToken.IsCancellationRequested)
                Execute.OnUIThread(() => ForceBanProcess(1));
            
            await AwaitStageAsync(stageInfo, finder, 2);

            if (!_scanningCancellationToken.IsCancellationRequested)
                Execute.OnUIThread(() => ForceBanProcess(2));

            await AwaitStageAsync(stageInfo, finder, 3);

            if (!_scanningCancellationToken.IsCancellationRequested)
                Execute.OnUIThread(() => ForceBanProcess(3));

            await AwaitStageAsync(stageInfo, finder, 4);

            if (!_scanningCancellationToken.IsCancellationRequested)
                Execute.OnUIThread(ForceFirstPickProcess);

            _isFirstAndSecondBanProcessing = false;
        }

        public void ToggleVisible()
        {
            var view = View;
            if (view.Visibility == Visibility.Visible)
            {
                Hide();
                SwitchMapSelector(false);
            }
            else
            {
                Show();
                SwitchMapSelector(true);
            }
        }

        public void Show()
        {
            var visibility = Visibility.Visible;
            _heroSelectorWindowViewModel.View.Visibility = visibility;

            if (_mapSelectorViewModel != null && _mapSelectorViewModel.View != null)
            {
                _mapSelectorViewModel.View.Visibility = visibility;
            }

            foreach (var heroSelectorViewModel in _cachedHeroSelectorViewModels)
            {
                heroSelectorViewModel.LayerVisible = true;
            }

            View.Visibility = visibility;
        }

        private void SwitchMapSelector(bool display, bool continueWithScan = true)
        {
            _mapSelectorViewModel.Visibility = display ? Visibility.Visible : Visibility.Hidden;
            SwitchAdviceWindow(display);
            if (display)
            {
                BpScreenLoaded = true;

                if (IsAutoMode && continueWithScan)
                {
                    _hasLookedForMap = true;
                    Task.Run(LookForMap).ConfigureAwait(false);
                }
            }
        }

        public void Hide()
        {
            var visibility = Visibility.Hidden;
            _heroSelectorWindowViewModel.View.Visibility = visibility;

            if (_mapSelectorViewModel != null && _mapSelectorViewModel.View != null)
            {
                _mapSelectorViewModel.View.Visibility = visibility;
            }

            foreach (var heroSelectorViewModel in _cachedHeroSelectorViewModels)
            {
                heroSelectorViewModel.LayerVisible = false;
            }

            View.Visibility = visibility;
        }

        private void SwitchAdviceWindow(bool display)
        {
            if (display && Width == Height)
            {
                var unitSize = App.MyPosition.BpHelperSize.ToUnitSize();
                Width = unitSize.Width;
                Height = unitSize.Height;
            }
        }

        private void SelectSide(BpStatus.Side side)
        {
            _mapSelectorViewModel.SelectSide(side);
        }

        protected virtual void OnRemindUserDetectMode()
        {
            RemindDetectMode?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRemindBpStart()
        {
            RemindBpStart?.Invoke(this, EventArgs.Empty);
        }

        public void CancelScan()
        {
            OnTurnOffAutoDetectMode();
        }

        protected virtual void OnTurnOffAutoDetectMode()
        {
            TurnOffAutoDetectMode?.Invoke(this, EventArgs.Empty);
        }

        public void WarnNotInBp()
        {
            Execute.OnUIThread(() => _toastService.ShowWarning(L("NotInBpQuestion")));
        }

        public void Handle(MapSelectedMessage message)
        {
            if (!BpStarted)
                return;

            var idList = new List<string>();

            foreach (var vm in HeroSelectorViewModels)
            {
                if (vm.SelectedItemInfo != null)
                    idList.Add(vm.SelectedItemInfo.Id);
            }

            BpStatus.Map = message.ItemInfo.Id;
            InvokeScript("update", new List<Tuple<string, string>>
                {
                    Tuple.Create("chose", string.Join("|", idList)),
                    Tuple.Create("map", BpStatus.Map),
                    Tuple.Create("lang", App.Language)
                });
        }
    }

    public class BpStatus
    {
        public enum Side
        {
            Left,

            Right
        }

        private int _currentStep;

        public BpStatus()
        {
            StepSelectedIndex = new HashSet<int>();
        }

        public Side FirstSide { get; set; }

        public string Map { get; set; }

        public int CurrentStep
        {
            get { return _currentStep; }
            set
            {
                if (_currentStep == value)
                    return;

                _currentStep = value;
                StepSelectedIndex = new HashSet<int>();
            }
        }

        public HashSet<int> StepSelectedIndex { get; set; }
    }
}