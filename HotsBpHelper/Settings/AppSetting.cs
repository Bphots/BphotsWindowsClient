using System.Windows;

namespace HotsBpHelper.Settings
{
    public class AppSetting
    {
        public Position[] Positions { get; set; }

        public Position MyPosition { get; set; }
    }

    public class Position
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public Point BpHelperPosition { get; set; }

        public Size BpHelperSize { get; set; }

        public Point MapSelectorPosition { get; set; }

        public SidePosition Left { get; set; }

        public SidePosition Right { get; set; }

        public int HeroWidth { get; set; }

        public int HeroHeight { get; set; }
    }

    public class SidePosition
    {
        public Point Ban1 { get; set; }

        public Point Ban2 { get; set; }

        public Point Pick1 { get; set; }

        public int Dx { get; set; }

        public int Dy { get; set; }

        public Point[] HeroPathPoints { get; set; }
    }
}