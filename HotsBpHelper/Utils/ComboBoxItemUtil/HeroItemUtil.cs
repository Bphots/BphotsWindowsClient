using System.Collections.Generic;
using System.Linq;
using HotsBpHelper.Api.Model;

namespace HotsBpHelper.Utils.ComboBoxItemUtil
{
    public class HeroItemUtil : IComboxItemUtil
    {
        private IEnumerable<ComboBoxItemInfo> _heroInfos;

        public IEnumerable<ComboBoxItemInfo> GetComboxItemInfos()
        {
            if (_heroInfos == null)
            {
                _heroInfos = HeroInfoV2.ToHeroInfoList(App.AdviceHeroInfos, App.Language)
                    .Select(hi => new ComboBoxItemInfo
                    {
                        Id = hi.Id.ToString(),
                        Name = hi.Name,
                        Acronym = hi.Acronym
                    })
                    .OrderBy(item => item.Name);
            }
            return _heroInfos;
        }
    }
}