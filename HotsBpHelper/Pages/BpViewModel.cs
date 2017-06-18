using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using HotsBpHelper.Settings;
using HotsBpHelper.UserControls;
using HotsBpHelper.Utils;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class BpViewModel : ViewModelBase
    {
        private readonly IHeroSelectorViewModelFactory _heroSelectorViewModelFactory;

        private readonly IEventAggregator _eventAggregator;

        public BindableCollection<HeroSelectorViewModel> HeroSelectorViewModels { get; set; }

        public int Left { get; set; }
        public int Top { get; set; }

        public Uri LocalFileUri { get; set; }

        public BpViewModel(IHeroSelectorViewModelFactory heroSelectorViewModelFactory, IEventAggregator eventAggregator)
        {
            _heroSelectorViewModelFactory = heroSelectorViewModelFactory;
            _eventAggregator = eventAggregator;
            HeroSelectorViewModels = new BindableCollection<HeroSelectorViewModel>();

            Left = (int) App.MyPosition.BpHelperPosition.X;
            Top = (int) App.MyPosition.BpHelperPosition.Y;
            

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

        public void Init()
        {
            InvokeScript("init", "zhm", 0, "zh-CN");
        }

        private void InvokeScript(string scriptName, params object[] args)
        {
            _eventAggregator.Publish(new InvokeScriptParameter()
            {
                ScriptName = scriptName,
                Args = args
            });
        }

        protected override void OnClose()
        {
            CloseHeroSelector();
            base.OnClose();
        }
    }

    public interface IHeroSelectorViewModelFactory
    {
        HeroSelectorViewModel CreateViewModel();
    }
}