using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HotsBpHelper.Api;

namespace HotsBpHelper.Utils.HeroUtil
{
    public class HeroUtil : IHeroUtil
    {
        private readonly IRestApi _restApi;

        private IEnumerable<HeroInfo> _heroInfos;

        public HeroUtil(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public IEnumerable<HeroInfo> GetHeroInfos()
        {
            if (_heroInfos == null)
            {
                var names = _restApi.GetHeroList(CultureInfo.CurrentCulture.Name);
                _heroInfos = names.Select(n => new HeroInfo()
                {
                    Id = n.Key,
                    FullName = n.Value,
                    ShortName = n.Value,
                }).OrderBy(hi => hi.FullName);
            }
            return _heroInfos;
        }
    }
}