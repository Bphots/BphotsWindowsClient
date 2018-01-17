using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HotsBpHelper.Messages;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;
using Size = System.Drawing.Size;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorViewModel : SelectorViewModel, IHandle<ItemSelectedMessage>
    {
        private Visibility _visibility = Visibility.Visible;

        public HeroSelectorViewModel(HeroItemUtil heroItemUtil, IEventAggregator eventAggregator) : base(heroItemUtil, eventAggregator)
        {
            Size = new Size(130, 20);
            EventAggregator.Subscribe(this);
        }

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                SetAndNotify(ref _visibility, value);
            }
        }
        
        protected override void OnViewLoaded()
        {
            if (Id == 0 || Id == 1 || Id == 7 || Id == 8)
            {
                // 禁选英雄选择,增加[未选择]

                ItemsInfos.Insert(0, new ComboBoxItemInfo()
                {
                    Id = "0",
                    Name = "None",
                    Acronym = "",
                });
            }
            base.OnViewLoaded();
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

        public void Handle(ItemSelectedMessage message)
        {
            // TODO 将已选的英雄移除(又改了之前的选择需要恢复)

        }


    }
}