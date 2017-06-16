using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.HeroUtil;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorViewModel : ViewModelBase
    {
        private readonly IHeroUtil _heroUtil;

        public int Left { get; set; }
        public int Top { get; set; }

        public Size Size { get; set; }

        public IEnumerable<HeroInfo> HeroInfos { get; set; }

        public HeroSelectorViewModel(IHeroUtil heroUtil)
        {
            _heroUtil = heroUtil;
            Size = new Size(150, 20);
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
}