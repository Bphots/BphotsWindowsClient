using System.Collections.Generic;
using System.Windows;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class MapSelectorViewModel : SelectorViewModel
    {
        public MapSelectorViewModel(MapItemUtil mapItemUtil, IEventAggregator eventAggregator) : base(mapItemUtil, eventAggregator)
        {
            Size = new Size(130, 20);
        }
    }
}