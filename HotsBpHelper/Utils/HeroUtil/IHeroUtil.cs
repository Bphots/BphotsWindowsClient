using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotsBpHelper.Utils.HeroUtil
{
    public interface IHeroUtil
    {
        IEnumerable<HeroInfo> GetHeroInfos();
    }
}