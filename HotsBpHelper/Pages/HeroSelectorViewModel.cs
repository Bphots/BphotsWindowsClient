using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorViewModel : SelectorViewModel
    {
        public HeroSelectorViewModel(HeroItemUtil heroItemUtil, IEventAggregator eventAggregator) : base(heroItemUtil, eventAggregator)
        {
            Size = new Size(130, 20);
        }
    }
}