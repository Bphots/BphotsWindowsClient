﻿using System.Linq;
using System.Windows;
using HotsBpHelper.Messages;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;
using Size = System.Drawing.Size;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorViewModel : SelectorViewModel, IHandle<ItemSelectedMessage>
    {
        private bool _interactionVisible = true;
        private bool _layerVisible = true;
        private object _isFocused;

        public HeroSelectorViewModel(HeroItemUtil heroItemUtil, IEventAggregator eventAggregator)
            : base(heroItemUtil, eventAggregator)
        {
            Size = new Size(120, 20);
            EventAggregator.Subscribe(this);

        }
        
        public bool InteractionVisible
        {
            get { return _interactionVisible; }
            set
            {
                SetAndNotify(ref _interactionVisible, value);
                NotifyOfPropertyChange(() => UserVisibility);
            }
        }

        public bool LayerVisible
        {
            get { return _layerVisible; }
            set
            {
                SetAndNotify(ref _layerVisible, value);
                NotifyOfPropertyChange(() => UserVisibility);
            }
        }

        public Visibility UserVisibility => LayerVisible && InteractionVisible ? Visibility.Visible : Visibility.Hidden;

        public object IsFocused
        {
            get { return _isFocused; }
            set { SetAndNotify(ref _isFocused, value); }
        }

        public void Handle(ItemSelectedMessage message)
        {
            // TODO 将已选的英雄移除(又改了之前的选择需要恢复)
        }

        public void InitializeUnselect()
        {
            if (Id == 0 || Id == 1 || Id == 2 || Id == 8 || Id == 9 || Id == 10)
            {
                // 禁选英雄选择,增加[未选择]
                ItemsInfos.Insert(0, new ComboBoxItemInfo
                {
                    Id = "0",
                    Name = L("NO_CHOOSE"),
                    Acronym = ""
                });
            }
        }

        public void Select(string name)
        {
            var selectedItem = ItemsInfos.FirstOrDefault(c => c.Name == name);
            if (selectedItem != null)
                SelectedItemInfo = selectedItem;
            else
            {
                ItemsInfos.Insert(0, new ComboBoxItemInfo
                {
                    Id = "0",
                    Name = name,
                    Acronym = ""
                });
                SelectedItemInfo = ItemsInfos[0];
            }
        }
    }
}