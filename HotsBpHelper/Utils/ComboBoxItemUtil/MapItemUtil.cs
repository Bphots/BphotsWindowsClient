using System.Collections.Generic;
using System.Linq;

namespace HotsBpHelper.Utils.ComboBoxItemUtil
{
    public class MapItemUtil : IComboxItemUtil
    {
        public IEnumerable<ItemInfo> GetComboxItemInfos()
        {
            return new List<ItemInfo>()
            {
                new ItemInfo
                {
                    Id = "tkd",
                    DisplayName = "天空殿",
                    ShortName = "tkd",
                },
                new ItemInfo
                {
                    Id = "jlz",
                    DisplayName = "巨龙镇",
                    ShortName = "jlz",
                },
                new ItemInfo
                {
                    Id = "lyst",
                    DisplayName = "炼狱圣坛",
                    ShortName = "lyst",
                },
                new ItemInfo
                {
                    Id = "yhzc",
                    DisplayName = "永恒战场",
                    ShortName = "yhzc",
                },
                new ItemInfo
                {
                    Id = "zhm",
                    DisplayName = "蛛后墓",
                    ShortName = "zhm",
                },
                new ItemInfo
                {
                    Id = "kmy",
                    DisplayName = "恐魔园",
                    ShortName = "kmy",
                },
                new ItemInfo
                {
                    Id = "zzg",
                    DisplayName = "诅咒谷",
                    ShortName = "zzg",
                },
                new ItemInfo
                {
                    Id = "hxw",
                    DisplayName = "黑心湾",
                    ShortName = "hxw",
                },
                new ItemInfo
                {
                    Id = "mrt",
                    DisplayName = "末日塔",
                    ShortName = "mrt",
                },
                new ItemInfo
                {
                    Id = "dtsnz",
                    DisplayName = "弹头枢纽站",
                    ShortName = "dtsnz",
                },
                new ItemInfo
                {
                    Id = "blkxsjq",
                    DisplayName = "布莱克西斯禁区",
                    ShortName = "blkxsjq",
                },
                new ItemInfo
                {
                    Id = "hc",
                    DisplayName = "花村",
                    ShortName = "hc",
                },
            }
            .OrderBy(m => m.DisplayName)
            ;
        }
    }
}