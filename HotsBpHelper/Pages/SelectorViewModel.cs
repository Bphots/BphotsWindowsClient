using System.Collections.ObjectModel;
using System.Drawing;
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

        protected ComboBoxItemInfo PSelectedItemInfo;

        protected SelectorViewModel(IComboxItemUtil comboxItemUtil, IEventAggregator eventAggregator)
        {
            ComboxItemUtil = comboxItemUtil;
            EventAggregator = eventAggregator;

            ItemsInfos = new ObservableCollection<ComboBoxItemInfo>(ComboxItemUtil.GetComboxItemInfos());
        }

        public int Id { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public Size Size { get; set; }

        public ObservableCollection<ComboBoxItemInfo> ItemsInfos { get; set; }

        public ComboBoxItemInfo SelectedItemInfo
        {
            get { return PSelectedItemInfo; }
            set { SetAndNotify(ref PSelectedItemInfo, value); }
        }

        public bool Selected { get; set; }

        public void ConfirmSelection()
        {
            EventAggregator.Publish(new ItemSelectedMessage
            {
                ItemInfo = PSelectedItemInfo,
                SelectorId = Id
            });

            Selected = true;
        }

        public void CancelSelection()
        {
            EventAggregator.Publish(new ItemSelectedMessage
            {
                ItemInfo = null,
                SelectorId = Id
            });
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
            var pos = new Point(unitPos.X - Size.Width/2, unitPos.Y);
            SetPosition(pos);
        }

        private void SetPosition(Point unitPosition)
        {
            Left = unitPosition.X;
            Top = unitPosition.Y;
        }
    }
}