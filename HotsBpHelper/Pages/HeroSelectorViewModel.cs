using System.Drawing;
using System.Linq;
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

        private void StartFinding()
        {
            Task.Run(() =>
            {
                while (PSelectedItemInfo == null || string.IsNullOrEmpty(PSelectedItemInfo.Name))
                {
                    string name = _heroFinder.FindHero(Id, new Point(12, 182)); // TODO 计算真实的英雄名称左上角坐标
                    if (!string.IsNullOrEmpty(name))
                    {
                        SelectedItemInfo = ItemsInfos.Single(item => item.Name == name);
                    }
                    Task.Delay(1000);
                }
            });
        }

        public void Handle(ItemSelectedMessage message)
        {
            // TODO 将已选的英雄移除(又改了之前的选择需要恢复)

        }
    }
}