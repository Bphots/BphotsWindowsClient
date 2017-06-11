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

        public async Task<IEnumerable<HeroInfo>> GetHeroInfosAsync()
        {
            if (_heroInfos == null)
            {
                var names = await _restApi.GetHeroList(CultureInfo.CurrentCulture.Name);
                _heroInfos = names.Select(n => new HeroInfo()
                {
                    FullName = n,
                    ShortName = n,
                });
            }
            return _heroInfos;
        }
    }
}