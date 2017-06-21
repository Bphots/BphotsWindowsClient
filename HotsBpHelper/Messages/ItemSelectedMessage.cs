using HotsBpHelper.Utils.ComboBoxItemUtil;

namespace HotsBpHelper.Messages
{
    public class ItemSelectedMessage
    {
        public ComboBoxItemInfo ItemInfo { get; set; }

        public int SelectorId { get; set; }
    }
}