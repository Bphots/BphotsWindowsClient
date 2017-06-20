using System.Collections.Generic;
using System.Windows;
using HotsBpHelper.Api.Model;
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

        public IEnumerable<ItemInfo> ItemsInfos { get; set; }

        private ItemInfo _selectedItemInfo;

        public ItemInfo SelectedItemInfo
        {
            get { return _selectedItemInfo; }
            set
            {
                SetAndNotify(ref _selectedItemInfo, value);
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

            ItemsInfos = ComboxItemUtil.GetComboxItemInfos();
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

        public void SetCenterAndTop(Point position)
        {
            var pos = new Point(position.X - Size.Width / 2, position.Y).ToPixelPoint();
            SetPosition(pos);
        }

        private void SetPosition(Point position)
        {
            Left = (int)position.X;
            Top = (int)position.Y;
        }
    }

    public class ItemSelectedMessage
    {
        public ItemInfo ItemInfo { get; set; }

        public int SelectorId { get; set; }
    }
}