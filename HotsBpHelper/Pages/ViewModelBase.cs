using Stylet;
using WPFLocalizeExtension.Extensions;

namespace HotsBpHelper.Pages
{
    public class ViewModelBase:Screen
    {
        static protected string L(string key)
        {
            return LocExtension.GetLocalizedValue<string>(key);
        }
    }
}