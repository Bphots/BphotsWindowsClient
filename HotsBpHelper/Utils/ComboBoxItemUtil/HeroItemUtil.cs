using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Model;

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
                _heroInfos = _restApi.GetHeroList(App.Language).OrderBy(hi => hi.Name);
            }
            return _heroInfos;
        }
    }
}