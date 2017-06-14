using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HotsBpHelper.Utils.HeroUtil;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorViewModel : ViewModelBase
    {
        private readonly IHeroUtil _heroUtil;

        public Point Location { get; set; }

        public IEnumerable<HeroInfo> HeroInfos { get; set; }

        public HeroSelectorViewModel(IHeroUtil heroUtil)
        {
            _heroUtil = heroUtil;
        }

        protected override void OnViewLoaded()
        {
            HeroInfos = _heroUtil.GetHeroInfos();
            base.OnViewLoaded();
        }
    }
}