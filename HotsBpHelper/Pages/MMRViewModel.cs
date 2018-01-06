using System;
using System.IO;
using System.Linq;
using System.Windows;
using HotsBpHelper.UserControls;
using HotsBpHelper.Utils;
using StatsFetcher;
using Stylet;
using Size = System.Drawing.Size;

namespace HotsBpHelper.Pages
{
    public class MMRViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        public Uri LocalFileUri { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public MMRViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            var size = new Size(450, 270).ToUnitSize();
            Width = size.Width;
            Height = size.Height;

            string filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "mmr.html#") + App.Language;
            LocalFileUri = new Uri(filePath, UriKind.Absolute);

        }

        protected override void OnViewLoaded()
        {
            View.Visibility = Visibility.Hidden;
        }

        public void ToggleVisible()
        {
            var view = View;
            Visibility visibility;
            if (view.Visibility == Visibility.Visible)
            {
                visibility = Visibility.Hidden;
            }
            else
            {
                visibility = Visibility.Visible;
            }
            View.Visibility = visibility;
        }

        public void FillMMR(Game game)
        {
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setAutoCloseSeconds",
                Args = new[] { App.AppSetting.MMRAutoCloseSeconds.ToString() }
            }, "MMRChanel");

            // 取得地区ID
            string regionId = ((int)game.Region).ToString();
            // 玩家BattleTags
            string battleTags = string.Join("|", game.Players
                .Select(p => p.BattleTag));
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setPlayers",
                Args = new[] { regionId, "left", battleTags }
            }, "MMRChanel");
        }
    }
}