using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotsBpHelper.Utils.HeroUtil
{
    public interface IHeroUtil
    {
        Task<IEnumerable<HeroInfo>> GetHeroInfosAsync();
    }
}