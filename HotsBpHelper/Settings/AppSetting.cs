using System;
using System.Drawing;

namespace HotsBpHelper.Settings
{
    public class AppSetting
    {

        public Position Position { get; set; }

        public Size DefaultBpHelperSize { get; set; }

        public int MMRAutoCloseSeconds { get; set; }
    }

    public class Position
    {
        public MapPosition MapPosition { get; set; }
        
        public int Width { get; set; }

        public int Height { get; set; }

        public Point BpHelperPosition { get; set; }

        public Size BpHelperSize { get; set; }

        public Point MapSelectorPosition { get; set; }

        public SidePosition Left { get; set; }

        public SidePosition Right { get; set; }

        public int HeroWidth { get; set; }
        
        public int HeroHeight { get; set; }

        public OverlapPoints OverlapPoints { get; set; }
    }

    public class OverlapPoints
    {
        public Point FrameRightBorderPoint { get; set; }

        public Point AppearanceFramePoint { get; set; }

        public Point SkillFramePoint { get; set; }

        public Point TalentFramePoint { get; set; }

        public Point FullChatHorizontalPoint { get; set; }

        public Point PartialChatlHorizontalPoint { get; set; }
    }

    public class MapPosition
    {
        public System.Drawing.Point Location { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }

    public class SidePosition
    {

        public Point Ban1 { get; set; }

        public Point Ban2 { get; set; }

        public Point Pick1 { get; set; }

        public int Dx { get; set; }

        public int Dy { get; set; }

        public Point[] HeroPathPoints { get; set; }

        public Point HeroName1 { get; set; }
    }
}