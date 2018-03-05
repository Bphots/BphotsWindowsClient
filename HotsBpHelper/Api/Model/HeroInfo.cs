using System.Collections.Generic;
using System.Linq;

namespace HotsBpHelper.Api.Model
{
    public class HeroInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Acronym { get; set; }
    }

    public class Tag
    {
        public string Key { get; set; }

        public int Value { get; set; }
    }

    public class HeroName
    {
        public string Full { get; set; }

        public string Short { get; set; }

        public string Acronym { get; set; }
    }

    public class HeroInfoV2
    {
        public List<Tag> Tags { get; set; }

        public Dictionary<string, HeroName> Name { get; set; }

        public int Id { get; set; }

        public string Basic { get; set; }

        public static HeroInfo ToHeroInfo(HeroInfoV2 heroInfoV2, string language)
        {
            var heroInfo = new HeroInfo {Id = heroInfoV2.Id};
            if (heroInfoV2.Name.ContainsKey(language))
            {
                heroInfo.Name = heroInfoV2.Name[language].Full;
                heroInfo.Acronym = heroInfoV2.Name[language].Acronym;
            }
            return heroInfo;
        }

        public static List<HeroInfo> ToHeroInfoList(Dictionary<int, HeroInfoV2> heroInfoV2List, string language)
        {
            return heroInfoV2List.Select(heroInfoV2 => ToHeroInfo(heroInfoV2.Value, language)).ToList();
        } 
    }
    
    public class LobbyHeroInfo
    {
        public bool IsNew { get; set; }

        public string Name { get; set; }
    }
}