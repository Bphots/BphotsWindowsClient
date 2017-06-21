using System.Collections.Generic;
using HotsBpHelper.Api.Model;

namespace HotsBpHelper.Utils.ComboBoxItemUtil
{
    public interface IComboxItemUtil
    {
        IEnumerable<ComboBoxItemInfo> GetComboxItemInfos();
    }
}