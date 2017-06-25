using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using HotsBpHelper.Api.Security;
using HotsBpHelper.Messages;
using HotsBpHelper.Settings;
using HotsBpHelper.UserControls;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class BpViewModel : ViewModelBase, IHandle<ItemSelectedMessage>, IHandle<SideSelectedMessage>
    {
        private readonly IHeroSelectorViewModelFactory _heroSelectorViewModelFactory;

        private readonly IMapSelectorViewModelFactory _mapSelectorViewModelFactory;

        private readonly IEventAggregator _eventAggregator;

        private readonly ISecurityProvider _securityProvider;

        public BindableCollection<HeroSelectorViewModel> HeroSelectorViewModels { get; set; } = new BindableCollection<HeroSelectorViewModel>();

        public int Left { get; set; }

        public int Top { get; set; }

        public Uri LocalFileUri { get; set; }

        public BpStatus BpStatus { get; set; }

        private IList<Point> _listPositions;

        private IList<IList<int>> _listBpSteps;


        public BpViewModel(IHeroSelectorViewModelFactory heroSelectorViewModelFactory,
            IMapSelectorViewModelFactory mapSelectorViewModelFactory,
            IEventAggregator eventAggregator, ISecurityProvider securityProvider)
        {
            _heroSelectorViewModelFactory = heroSelectorViewModelFactory;
            _mapSelectorViewModelFactory = mapSelectorViewModelFactory;
            _eventAggregator = eventAggregator;
            _securityProvider = securityProvider;

            _eventAggregator.Subscribe(this);

            Left = (int)App.MyPosition.BpHelperPosition.X;
            Top = (int)App.MyPosition.BpHelperPosition.Y;

            string filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "index.html");
            LocalFileUri = new Uri(filePath, UriKind.Absolute);
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            FillPositions();
            ShowMapSelector();
        }

        private void ShowMapSelector()
        {
            var vm = _mapSelectorViewModelFactory.CreateViewModel();
            vm.Id = 0;
            vm.SetCenterAndTop(App.MyPosition.MapSelectorPosition);
/*
            _eventAggregator.Publish(new ShowWindowMessage
            {
                ViewModel = vm,
            });
*/
            WindowManager.ShowWindow(vm);
        }

        public void ShowHeroSelector(int pointIndex)
        {
            var vm = _heroSelectorViewModelFactory.CreateViewModel();
            HeroSelectorViewModels.Add(vm);
            vm.Id = pointIndex;
            var position = _listPositions[pointIndex];
            if (pointIndex < 7)
            {
                vm.SetLeftAndTop(position);
            }
            else
            {
                vm.SetRightAndTop(position);
            }
            _eventAggregator.Publish(new ShowWindowMessage
            {
                ViewModel = vm,
            });
            WindowManager.ShowWindow(vm);
        }

        public void FillPositions()
        {
            SidePosition sidePosition;
            double x, y;
            int dx, dy;
            _listPositions = new List<Point>(14); // BP总共14个选择

            // Left
            sidePosition = App.MyPosition.Left;
            _listPositions.Add(sidePosition.Ban1);
            _listPositions.Add(sidePosition.Ban2);
            x = sidePosition.Pick1.X;
            y = sidePosition.Pick1.Y;
            dx = sidePosition.Dx;
            dy = sidePosition.Dy;
            for (int i = 0; i < 5; i++)
            {
                _listPositions.Add(new Point(x, y));
                x += dx;
                y += dy;
                dx = -dx;
            }
            // Right
            sidePosition = App.MyPosition.Right;
            _listPositions.Add(sidePosition.Ban1);
            _listPositions.Add(sidePosition.Ban2);
            x = sidePosition.Pick1.X;
            y = sidePosition.Pick1.Y;
            dx = sidePosition.Dx;
            dy = sidePosition.Dy;
            for (int i = 0; i < 5; i++)
            {
                _listPositions.Add(new Point(x, y));
                x += dx;
                y += dy;
                dx = -dx;
            }
        }

        public void CloseHeroSelector()
        {
            foreach (var vm in HeroSelectorViewModels)
            {
                vm.RequestClose();
            }
            HeroSelectorViewModels.Clear();
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
            });
        }

        protected override void OnClose()
        {
            CloseHeroSelector();
            base.OnClose();
        }

        public void Handle(ItemSelectedMessage message)
        {
            InvokeScript("update", new List<Tuple<string, string>>
                    {
                        Tuple.Create("chose", string.Join("|",
                            HeroSelectorViewModels
                            .Where(vm => vm.SelectedItemInfo != null)
                            .Select(vm => vm.SelectedItemInfo.Id))),
                        Tuple.Create("map", BpStatus.Map),
                        Tuple.Create("lang", App.Language)
                    });
            if (BpStatus.StepSelectedIndex.Contains(message.SelectorId) ||  // 修改本轮选过的英雄
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
                    BpStatus.CurrentStep++;
                    ProcessStep();
                }
            }
        }

        public void Handle(SideSelectedMessage message)
        {
            // 初始化BP过程
            BpStatus = new BpStatus()
            {
                Map = message.ItemInfo.Id,
                FirstSide = message.Side,
            };

            int side = (int)BpStatus.FirstSide;
            InvokeScript("init", message.ItemInfo.Id, side.ToString(), App.Language);
            InvokeScript("update", new List<Tuple<string, string>>
                    {
                        Tuple.Create("chose", ""),
                        Tuple.Create("map", BpStatus.Map),
                        Tuple.Create("lang", App.Language)
                    });

            CloseHeroSelector();
            // Position下标位置示意
            //   0 1   7 8
            // 2          9
            // 3          10
            // 4          11
            // 5          12
            // 6          13

            if (message.Side == BpStatus.Side.Left)
            {
                _listBpSteps = new List<IList<int>> // BP总共10手
                {
                    new List<int> {0},
                    new List<int> {7},
                    new List<int> {2},
                    new List<int> {9, 10},
                    new List<int> {3, 4},
                    new List<int> {8},
                    new List<int> {1},
                    new List<int> {11, 12},
                    new List<int> {5, 6},
                    new List<int> {13},
                };
            }
            else
            {
                _listBpSteps = new List<IList<int>> // BP总共10手
                {
                    new List<int> {7},
                    new List<int> {0},
                    new List<int> {9},
                    new List<int> {2, 3},
                    new List<int> {10, 11},
                    new List<int> {1},
                    new List<int> {8},
                    new List<int> {4, 5},
                    new List<int> {12, 13},
                    new List<int> {6},
                };
            }
            BpStatus.CurrentStep = 0;
            ProcessStep();
        }

        private void ProcessStep()
        {
            if (BpStatus.CurrentStep == _listBpSteps.Count) return;
            // 显示英雄选择框
            foreach (var i in _listBpSteps[BpStatus.CurrentStep])
            {
                ShowHeroSelector(i);
            }
            BpStatus.StepSelectedIndex = new HashSet<int>();
        }
    }

    public interface IHeroSelectorViewModelFactory
    {
        HeroSelectorViewModel CreateViewModel();
    }

    public interface IMapSelectorViewModelFactory
    {
        MapSelectorViewModel CreateViewModel();
    }

    public class BpStatus
    {
        public enum Side
        {
            Left,

            Right
        }

        public Side FirstSide { get; set; }

        public string Map { get; set; }

        public int CurrentStep { get; set; }

        public HashSet<int> StepSelectedIndex { get; set; }
    }
}