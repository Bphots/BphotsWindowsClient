using System.Collections.Generic;
using System.Windows;
using HotsBpHelper.Messages;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class MapSelectorViewModel : SelectorViewModel
    {
        public MapSelectorViewModel(MapItemUtil mapItemUtil, IEventAggregator eventAggregator) : base(mapItemUtil, eventAggregator)
        {
            Size = new Size(178, 24);//修改地图框大小后，需要修改此项进行匹配
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == "CanSelectSide")
            {
                EventAggregator.Publish(new SideSelectedMessage()
                {
                    ItemInfo = null,
                    Side = BpStatus.Side.Left,
                });
            }
            base.OnPropertyChanged(propertyName);
        }

        public new ComboBoxItemInfo SelectedItemInfo
        {
            get { return base.SelectedItemInfo; }
            set
            {
                SetAndNotify(ref PSelectedItemInfo, value);
                NotifyOfPropertyChange(propertyName: nameof(CanSelectSide));
            }
        }

        public void SelectSide(BpStatus.Side side)
        {
            EventAggregator.Publish(new SideSelectedMessage()
            {
                ItemInfo = SelectedItemInfo,
                Side = side,
            });
        }

        public bool CanSelectSide => SelectedItemInfo != null;
    }
}