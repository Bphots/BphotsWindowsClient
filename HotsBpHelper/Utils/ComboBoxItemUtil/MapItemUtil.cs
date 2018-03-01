using System.Collections.Generic;
using System.Linq;
using HotsBpHelper.Api.Model;

namespace HotsBpHelper.Utils.ComboBoxItemUtil
{
    public class MapItemUtil : IComboxItemUtil
    {
        private IEnumerable<ComboBoxItemInfo> _mapInfos;

        public IEnumerable<ComboBoxItemInfo> GetComboxItemInfos()
        {
            if (_mapInfos == null)
            {
                _mapInfos = MapInfoV2.ToMapInfoList(App.AdviceMapInfos, App.Language)
                    .Select(mi => new ComboBoxItemInfo
                    {
                        Id = mi.Code,
                        Name = mi.Name,
                        Acronym = mi.Code
                    })
                    .OrderBy(mi => mi.Name);
            }
            return _mapInfos;
        }
    }
}