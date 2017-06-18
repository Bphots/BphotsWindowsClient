using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using HotsBpHelper.Settings;
using HotsBpHelper.Utils;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class BpViewModel : ViewModelBase
    {
        private readonly IHeroSelectorViewModelFactory _heroSelectorViewModelFactory;

        public BindableCollection<HeroSelectorViewModel> HeroSelectorViewModels { get; set; }

        public int Left { get; set; }
        public int Top { get; set; }

        public Uri LocalFileUri { get; set; }

        public BpViewModel(IHeroSelectorViewModelFactory heroSelectorViewModelFactory)
        {
            _heroSelectorViewModelFactory = heroSelectorViewModelFactory;
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

        public void Advise()
        {
            const string fmt = "http://www.bphots.com/bp_helper/advice?map={0}&timestamp={1}&nonce={2}&client_patch={3}&sign={4}";
            string map = "zhm";
            string timestamp = DateTime.Now.ToUnixTimestamp().ToString();
            string nonce = Guid.NewGuid().ToString().Substring(0, 8);
            string client_patch = "17060801";
            string param = "{map:'zhm'}";
            string key = "I7@gPm2F4HAcz@ak";
            string sign = Md5Util.CaculateStringMd5($"{key}-{timestamp}-{client_patch}-{nonce}-{param}");

            string url = string.Format(fmt, map, timestamp, nonce, client_patch, sign);
            LocalFileUri  = new Uri(url);
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