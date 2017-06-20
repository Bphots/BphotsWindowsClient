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

        private IEnumerable<ItemInfo> _mapInfos;

        public MapItemUtil(RestApi restApi)
        {
            _restApi = restApi;
        }

        public IEnumerable<ItemInfo> GetComboxItemInfos()
        {
            if (_mapInfos == null)
            {
                _mapInfos = _restApi.GetMapList(App.Language).OrderBy(mi => mi.Name);
            }
            return _mapInfos;
        }
    }
}