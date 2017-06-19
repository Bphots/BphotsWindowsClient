using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.HeroUtil;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorViewModel : ViewModelBase
    {
        private readonly IHeroUtil _heroUtil;

        private readonly IEventAggregator _eventAggregator;

        public int Id { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public Size Size { get; set; }

        public IEnumerable<HeroInfo> HeroInfos { get; set; }

        private HeroInfo _selectedHeroInfo;
        public HeroInfo SelectedHeroInfo
        {
            get { return _selectedHeroInfo; }
            set
            {
                SetAndNotify(ref _selectedHeroInfo, value);
                _eventAggregator.Publish(new HeroSelectedMessage
                {
                    HeroInfo = value,
                    SelectorId = Id,
                });
            }
        }

        public HeroSelectorViewModel(IHeroUtil heroUtil, IEventAggregator eventAggregator)
        {
            _heroUtil = heroUtil;
            _eventAggregator = eventAggregator;
            Size = new Size(130, 20);
            HeroInfos = _heroUtil.GetHeroInfos();
        }

        public void SetLeftAndTop(Point position)
        {
            var pos = position.ToPixelPoint();
            SetPosition(pos);
        }

        public void SetRightAndTop(Point position)
        {
            var pos = new Point(position.X - Size.Width, position.Y).ToPixelPoint();
            SetPosition(pos);
        }

        private void SetPosition(Point position)
        {
            Left = (int)position.X;
            Top = (int)position.Y;
        }
    }

    public class HeroSelectedMessage
    {
        public HeroInfo HeroInfo { get; set; }

        public int SelectorId { get; set; }
    }
}