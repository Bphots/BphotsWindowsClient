using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HotsBpHelper.Api;

namespace HotsBpHelper.Utils.ComboBoxItemUtil
{
    public class HeroItemUtil : IComboxItemUtil
    {
        private readonly IRestApi _restApi;

        private IEnumerable<ItemInfo> _heroInfos;

        public HeroItemUtil(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public IEnumerable<ItemInfo> GetComboxItemInfos()
        {
            if (_heroInfos == null)
            {
                var names = _restApi.GetHeroList(CultureInfo.CurrentCulture.Name);
                _heroInfos = names.Select(n => new ItemInfo()
                {
                    Id = n.Key.ToString(),
                    DisplayName = n.Value,
                    ShortName = n.Value,
                }).OrderBy(hi => hi.DisplayName);
            }
            return _heroInfos;
        }
    }
}