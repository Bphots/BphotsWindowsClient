using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HotsBpHelper.Messages;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorViewModel : SelectorViewModel, IHandle<ItemSelectedMessage>
    {
        public HeroSelectorViewModel(HeroItemUtil heroItemUtil, IEventAggregator eventAggregator) : base(heroItemUtil, eventAggregator)
        {
            Size = new Size(130, 20);
            EventAggregator.Subscribe(this);
        }

        protected override void OnViewLoaded()
        {
            if (Id == 0 || Id == 1 || Id == 7 || Id == 8)
            {
                // 禁选英雄选择,增加[未选择]

                ItemsInfos.Insert(0, new ComboBoxItemInfo()
                {
                    Id = "",
                    Name = L("NO_CHOOSE"),
                    Acronym = "",
                });
            }
            base.OnViewLoaded();
        }

        public void Handle(ItemSelectedMessage message)
        {
            // TODO 将已选的英雄移除(又改了之前的选择需要恢复)

        }
    }
}