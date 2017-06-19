using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using HotsBpHelper.Api.Security;
using HotsBpHelper.Settings;
using HotsBpHelper.UserControls;
using HotsBpHelper.Utils;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class BpViewModel : ViewModelBase, IHandle<HeroSelectedMessage>
    {
        private readonly IHeroSelectorViewModelFactory _heroSelectorViewModelFactory;

        private readonly IEventAggregator _eventAggregator;

        private readonly ISecurityProvider _securityProvider;

        public BindableCollection<HeroSelectorViewModel> HeroSelectorViewModels { get; set; }

        public int Left { get; set; }
        public int Top { get; set; }

        public Uri LocalFileUri { get; set; }

        private string _map = "zhm";

        private string _language = CultureInfo.CurrentCulture.Name;

        public BpViewModel(IHeroSelectorViewModelFactory heroSelectorViewModelFactory, IEventAggregator eventAggregator, ISecurityProvider securityProvider)
        {
            _heroSelectorViewModelFactory = heroSelectorViewModelFactory;
            _eventAggregator = eventAggregator;
            _securityProvider = securityProvider;

            _eventAggregator.Subscribe(this);
            HeroSelectorViewModels = new BindableCollection<HeroSelectorViewModel>();

            Left = (int)App.MyPosition.BpHelperPosition.X;
            Top = (int)App.MyPosition.BpHelperPosition.Y;

            string filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "index.html");
            LocalFileUri = new Uri(filePath, UriKind.Absolute);

        }

        public void ShowHeroSelector()
        {
            List<Point> lstPoints;
            SidePosition sidePosition;
            double x, y;
            int dx, dy;

            // Left
            lstPoints = new List<Point>(7);
            sidePosition = App.MyPosition.Left;
            lstPoints.Add(sidePosition.Ban1);
            lstPoints.Add(sidePosition.Ban2);
            x = sidePosition.Pick1.X;
            y = sidePosition.Pick1.Y;
            dx = sidePosition.Dx;
            dy = sidePosition.Dy;
            for (int i = 0; i < 5; i++)
            {
                lstPoints.Add(new Point(x, y));
                x += dx;
                y += dy;
                dx = -dx;
            }
            foreach (var point in lstPoints)
            {
                var vm = _heroSelectorViewModelFactory.CreateViewModel();
                HeroSelectorViewModels.Add(vm);
                vm.Id = HeroSelectorViewModels.Count;
                vm.SetLeftAndTop(point);
                WindowManager.ShowWindow(vm);
            }
            // Left
            lstPoints = new List<Point>(7);
            sidePosition = App.MyPosition.Right;
            lstPoints.Add(sidePosition.Ban1);
            lstPoints.Add(sidePosition.Ban2);
            x = sidePosition.Pick1.X;
            y = sidePosition.Pick1.Y;
            dx = sidePosition.Dx;
            dy = sidePosition.Dy;
            for (int i = 0; i < 5; i++)
            {
                lstPoints.Add(new Point(x, y));
                x += dx;
                y += dy;
                dx = -dx;
            }
            foreach (var point in lstPoints)
            {
                var vm = _heroSelectorViewModelFactory.CreateViewModel();
                HeroSelectorViewModels.Add(vm);
                vm.Id = HeroSelectorViewModels.Count;
                vm.SetRightAndTop(point);
                WindowManager.ShowWindow(vm);
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
            _eventAggregator.Publish(new InvokeScriptMessage()
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
            _eventAggregator.Publish(new InvokeScriptMessage()
            {
                ScriptName = scriptName,
                Args = parameters.Select(tuple => tuple.Item2).ToArray(),
            });
        }

        protected override void OnClose()
        {
            CloseHeroSelector();
            base.OnClose();
        }

        public void Handle(HeroSelectedMessage message)
        {
            switch (message.SelectorId)
            {
                case 1:
                case 8:
                    // 首BAN
                    var side = message.SelectorId == 1 ? "0" : "1";
                    InvokeScript("init", _map, side, _language);
                    InvokeScript("update", new List<Tuple<string, string>>()
                    {
                        Tuple.Create("chose", ""),
                        Tuple.Create("map",_map),
                        Tuple.Create("lang",_language),
                    });
                    break;
            }
        }
    }

    public interface IHeroSelectorViewModelFactory
    {
        HeroSelectorViewModel CreateViewModel();
    }
}