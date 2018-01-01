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
        public Uri LocalFileUri { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public MMRViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Width = 300;
            Height = 200;

            string filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "mmr.html#") + App.Language;
            LocalFileUri = new Uri(filePath, UriKind.Absolute);
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
            string regionId = ((int) game.Region).ToString();
            string leftBattleTags = string.Join("|", game.Players
                .Where(p => p.Team == 0)
                .Select(p => p.BattleTag));
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setPlayer",
                Args = new []{regionId, "left", leftBattleTags}
            }, "MMRChanel");

            string rightBattleTags = string.Join("|", game.Players
                .Where(p => p.Team == 1)
                .Select(p => p.BattleTag));
            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setPlayer",
                Args = new []{regionId, "right", rightBattleTags }
            }, "MMRChanel");
        }
    }
}