using HotsBpHelper.Pages;
using HotsBpHelper.Utils.ComboBoxItemUtil;

namespace HotsBpHelper.Messages
{
    public class SideSelectedMessage
    {
        public ComboBoxItemInfo ItemInfo { get; set; }

        public BpStatus.Side Side { get; set; }
    }
}