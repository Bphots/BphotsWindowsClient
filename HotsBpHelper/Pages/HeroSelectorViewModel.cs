using System.Windows;
using HotsBpHelper.Utils;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorViewModel : ViewModelBase
    {

        public Point Location { get; set; }


        public HeroSelectorViewModel(Point unitPosition)
        {
            Location = unitPosition.ToPixelPoint();
        }
    }
}