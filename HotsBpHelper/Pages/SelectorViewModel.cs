using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Messages;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;

namespace HotsBpHelper.Pages
{
    public abstract class SelectorViewModel : ViewModelBase
    {
        protected readonly IComboxItemUtil ComboxItemUtil;

        protected readonly IEventAggregator EventAggregator;

        public int Id { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public Size Size { get; set; }

        public ObservableCollection<ComboBoxItemInfo> ItemsInfos { get; set; }

        protected ComboBoxItemInfo PSelectedItemInfo;

        public ComboBoxItemInfo SelectedItemInfo
        {
            get { return PSelectedItemInfo; }
            set
            {
                SetAndNotify(ref PSelectedItemInfo, value);
                if (value == null) return;
                EventAggregator.Publish(new ItemSelectedMessage
                {
                    ItemInfo = value,
                    SelectorId = Id
                });
            }
        }

        protected SelectorViewModel(IComboxItemUtil comboxItemUtil, IEventAggregator eventAggregator)
        {
            ComboxItemUtil = comboxItemUtil;
            EventAggregator = eventAggregator;

            ItemsInfos = new ObservableCollection<ComboBoxItemInfo>(ComboxItemUtil.GetComboxItemInfos());
        }

        public void SetLeftAndTop(Point position)
        {
            var pos = position.ToUnitPoint();
            SetPosition(pos);
        }

        public void SetRightAndTop(Point position)
        {
            var unitPos = position.ToUnitPoint();
            var pos = new Point(unitPos.X - Size.Width, unitPos.Y);
            SetPosition(pos);
        }

        public void SetCenterAndTop(Point position)
        {
            var unitPos = position.ToUnitPoint();
            var pos = new Point(unitPos.X - Size.Width / 2, unitPos.Y);
            SetPosition(pos);
        }

        private void SetPosition(Point unitPosition)
        {
            Left = (int)unitPosition.X;
            Top = (int)unitPosition.Y;
        }
    }
}