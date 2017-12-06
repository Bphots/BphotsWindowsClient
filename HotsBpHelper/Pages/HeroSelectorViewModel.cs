using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotsBpHelper.HeroFinder;
using HotsBpHelper.Messages;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorViewModel : SelectorViewModel, IHandle<ItemSelectedMessage>
    {
        private readonly IHeroFinder _heroFinder;

        public HeroSelectorViewModel(HeroItemUtil heroItemUtil, IEventAggregator eventAggregator, IHeroFinder heroFinder) : base(heroItemUtil, eventAggregator)
        {
            _heroFinder = heroFinder;
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
            StartFinding();
            base.OnViewLoaded();
        }

        public new ComboBoxItemInfo SelectedItemInfo
        {
            get { return base.SelectedItemInfo; }
            set
            {
                SetAndNotify(ref PSelectedItemInfo, value);
            }
        }

        private void StartFinding()
        {
            Task.Run(() =>
            {
                while (PSelectedItemInfo == null || string.IsNullOrEmpty(PSelectedItemInfo.Name))
                {
                    string name = _heroFinder.FindHero(Id);
                    if (!string.IsNullOrEmpty(name))
                    {
                        SetAndNotify(ref PSelectedItemInfo, ItemsInfos.Single(item => item.Name == name), nameof(SelectedItemInfo));
                        Thread.Sleep(1500);
                        Execute.OnUIThread(base.ConfirmSelection);
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        public override void ConfirmSelection()
        {
            base.ConfirmSelection();
            // 手动选择,增加新模板
            _heroFinder.AddNewTemplate(Id, SelectedItemInfo.Name);
        }

        public void Handle(ItemSelectedMessage message)
        {
            // TODO 将已选的英雄移除(又改了之前的选择需要恢复)
        }
    }
}