using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace HotsBpHelper.Api.Model
{
    public class MapInfo
    {
        public string Name { get; set; }

        public string Code { get; set; }

    }

    public class MapInfoV2
    {
        public Dictionary<string, string> Name { get; set; }

        public string Code { get; set; }

        public static MapInfo ToMapInfo(MapInfoV2 mapInfoV2, string language)
        {
            var mapInfo = new MapInfo();
            if (mapInfoV2.Name.ContainsKey(language))
            {
                mapInfo.Name = mapInfoV2.Name[language];
                mapInfo.Code = mapInfoV2.Code;
            }
            return mapInfo;
        }

        public static List<MapInfo> ToMapInfoList(Dictionary<string, MapInfoV2> mapInfoV2List, string language)
        {
            return mapInfoV2List.Select(heroInfoV2 => ToMapInfo(heroInfoV2.Value, language)).ToList();
        }
    }
}