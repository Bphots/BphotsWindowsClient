using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using HotsBpHelper.UserControls;
using HotsBpHelper.Utils;
using LobbyFileParser;
using Stylet;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

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

            var location = new Point(App.AppSetting.Position.Width / 2 - App.AppSetting.Position.MmrWidth / 2, App.AppSetting.Position.Height / 2 - App.AppSetting.Position.MmrHeight / 2).ToUnitPoint();
            Left = location.X;
            Top = location.Y;
            
            var filePath = Path.Combine(App.AppPath, Const.LOCAL_WEB_FILE_DIR, "mmr.html#") + App.Language;
            LocalFileUri = filePath;
            WebCallbackListener.LobbyRequested += WebCallbackListenerOnLobbyRequested;
        }

        private void WebCallbackListenerOnLobbyRequested(object sender, EventArgs eventArgs)
        {
            if (!File.Exists(Const.BattleLobbyPath))
                return;

            var lobbyProcessor = new LobbyFileProcessor(Const.BattleLobbyPath, App.LobbyHeroes, App.LobbyMaps);
            var game = lobbyProcessor.ParseLobbyInfo();
            FillMMR(game);
            Show();
        }

        public string LocalFileUri { get; set; }

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
            int autoCloseSec = App.CustomConfigurationSettings.MMRAutoCloseTime;
            if (autoCloseSec > 0)
            {
                _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
                {
                    ScriptName = "setAutoCloseSeconds",
                    Args = new[] { autoCloseSec.ToString() }
                }, "MMRChanel");
            }

            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setLanguageForMessage",
                Args = new[] { App.CustomConfigurationSettings.LanguageForMessage }
            }, "MMRChanel");

            // 取得地区ID
            var regionId = ((int) game.Region).ToString();
            // 玩家BattleTags
            var battleTags = string.Join("|", game.Players
                .Select(p => p.Tag + "#" + p.SelectedHero));
            var players = game.Players.Select(p => p.Tag).ToList();

            var defaultPlayerIndex = GetDefaultPlayerIndex(players);

            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setMap",
                Args = new[] { game.Map }
            }, "MMRChanel");

            _eventAggregator.PublishOnUIThread(new InvokeScriptMessage
            {
                ScriptName = "setPlayers",
                Args = new[] {regionId, defaultPlayerIndex.ToString(), battleTags}
            }, "MMRChanel");
        }

        private int GetDefaultPlayerIndex(List<string> players)
        {
            var defaultPlayer = players.First();
            if (App.CustomConfigurationSettings.PlayerTags.Any(players.Contains))
            {
                defaultPlayer = App.CustomConfigurationSettings.PlayerTags.First(players.Contains);
                App.CustomConfigurationSettings.PlayerTags.Remove(defaultPlayer);
                App.CustomConfigurationSettings.PlayerTags.Insert(0, defaultPlayer);
                // for config.ini
                App.NextConfigurationSettings.PlayerTags.Remove(defaultPlayer);
                App.NextConfigurationSettings.PlayerTags.Insert(0, defaultPlayer);
            }
            else if (LastMatchPlayers.Count(players.Contains) == 1)
            {
                defaultPlayer = LastMatchPlayers.First(players.Contains);
                App.CustomConfigurationSettings.PlayerTags.Insert(0, defaultPlayer);
                // for config.ini
                App.NextConfigurationSettings.PlayerTags.Insert(0, defaultPlayer);
            }

            int defaultPlayerIndex = players.IndexOf(defaultPlayer);
            LastMatchPlayers.Clear();
            LastMatchPlayers.AddRange(players);
            return defaultPlayerIndex;
        }

        private static List<string> LastMatchPlayers { get; set; } = new List<string>();
        
        public void HideBrowser()
        {
            Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            var size = new Size(App.AppSetting.Position.MmrWidth, App.AppSetting.Position.MmrHeight).ToUnitSize();
            Width = size.Width;
            Height = size.Height;
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