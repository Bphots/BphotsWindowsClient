using System;
using System.IO;
using System.Linq;
using System.Windows;
using HotsBpHelper.UserControls;
using StatsFetcher;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class MMRViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private int _height;
        private Visibility _visibility;
        private int _width;

        public MMRViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Left = App.AppSetting.Position.Width/2 - 225;
            Top = App.AppSetting.Position.Height/2 - 135;

            //var unitPos = App.AppSetting.Position.BpHelperPosition.ToUnitPoint();
            //Left = unitPos.X;
            //Top = unitPos.Y;

            var filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "mmr.html#") + App.Language;
            LocalFileUri = new Uri(filePath, UriKind.Absolute);
        }

        public Uri LocalFileUri { get; set; }

        public int Width
        {
            get { return _width; }
            set { SetAndNotify(ref _width, value); }
        }

        public int Left { get; set; }

        public int Top { get; set; }

        public int Height
        {
            get { return _height; }
            set { SetAndNotify(ref _height, value); }
        }

        public Visibility Visibility
        {
            get { return _visibility; }
            set { SetAndNotify(ref _visibility, value); }
        }


        public void ToggleVisible()
        {
            var view = View;
            if (view.Visibility == Visibility.Visible)
                Hide();
            else
                Show();
        }

        public void FillMMR(Game game)
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setAutoCloseSeconds",
                Args = new[] {App.AppSetting.MMRAutoCloseSeconds.ToString()}
            }, "MMRChanel");

            // 取得地区ID
            var regionId = ((int) game.Region).ToString();
            // 玩家BattleTags
            var battleTags = string.Join("|", game.Players
                .Select(p => p.BattleTag));
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setPlayers",
                Args = new[] {regionId, "left", battleTags}
            }, "MMRChanel");
        }

        public void HideBrowser()
        {
            Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            Width = 450;
            Height = 270;
            Visibility = Visibility.Visible;
            View.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            View.Visibility = Visibility.Hidden;
            //Width = 0;
            //Height = 0;
            Visibility = Visibility.Hidden;
        }
    }
}