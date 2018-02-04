using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HotsBpHelper.Messages;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;
using Size = System.Drawing.Size;

namespace HotsBpHelper.Pages
{
    public class MapSelectorViewModel : SelectorViewModel
    {
        private Visibility _visibility;
        private Visibility _buttonVisibility;

        public MapSelectorViewModel(MapItemUtil mapItemUtil, IEventAggregator eventAggregator) : base(mapItemUtil, eventAggregator)
        {
            Size = new Size(178, 24);//修改地图框大小后，需要修改此项进行匹配
        }

        public void Select(string name)
        {
            var selectedItem = ItemsInfos.FirstOrDefault(c => c.Name == name);
            if (selectedItem != null)
                SelectedItemInfo = selectedItem;
            else
            {
                ItemsInfos.Insert(0, new ComboBoxItemInfo()
                {
                    Id = "",
                    Name = name,
                    Acronym = "",
                });
                SelectedItemInfo = ItemsInfos[0];
            }
        }
        
        public new ComboBoxItemInfo SelectedItemInfo
        {
            get { return base.SelectedItemInfo; }
            set
            {
                if (SetAndNotify(ref PSelectedItemInfo, value))
                {
                    EventAggregator.Publish(new MapSelectedMessage()
                    {
                        ItemInfo = value,
                    });
                    NotifyOfPropertyChange(() => CanSelectSide);
                }

            }
        }

        public void SelectSide(BpStatus.Side side)
        {
            EventAggregator.Publish(new SideSelectedMessage()
            {
                ItemInfo = null,
                Side = BpStatus.Side.Left,
            });
            EventAggregator.Publish(new SideSelectedMessage()
            {
                ItemInfo = SelectedItemInfo,
                Side = side,
            });
        }

        public bool CanSelectSide => SelectedItemInfo != null;

        public Visibility Visibility
        {
            get { return _visibility; }
            set { SetAndNotify(ref _visibility, value); }
        }

        public Visibility ButtonVisibility
        {
            get { return _buttonVisibility; }
            set { SetAndNotify(ref _buttonVisibility, value); }
        }
    }
}