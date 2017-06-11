using System.Collections.Generic;
using System.Windows;
using HotsBpHelper.Api;
using HotsBpHelper.Localization;
using HotsBpHelper.Utils;
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

        protected override async void OnViewLoaded()
        {
            HeroInfos = await _heroUtil.GetHeroInfosAsync();
            base.OnViewLoaded();
        }
    }
}