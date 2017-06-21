using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Model;

namespace HotsBpHelper.Utils.ComboBoxItemUtil
{
    public class MapItemUtil : IComboxItemUtil
    {
        private readonly RestApi _restApi;

        private IEnumerable<ComboBoxItemInfo> _mapInfos;

        public MapItemUtil(RestApi restApi)
        {
            _restApi = restApi;
        }

        public IEnumerable<ComboBoxItemInfo> GetComboxItemInfos()
        {
            if (_mapInfos == null)
            {
                _mapInfos = _restApi.GetMapList(App.Language)
                    .Select(mi => new ComboBoxItemInfo()
                    {
                        Id = mi.Code,
                        Name = mi.Name,
                        Acronym = mi.Name,
                    })
                    .OrderBy(mi => mi.Name);
            }
            return _mapInfos;
        }
    }
}