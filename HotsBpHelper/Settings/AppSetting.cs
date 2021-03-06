using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace HotsBpHelper.Settings
{
    public class AppSetting
    {
        public Position Position { get; set; }
    }

    public class Position
    {
        public int TeamLeagueDy { get; set; }

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

        public LoaddingPoints LoadingPoints { get; set; }

        public int MmrWidth;

        public int MmrHeight;

        public List<Rectangle> BanPositions { get; set; } 
    }

    public class LoaddingPoints
    {
        public Point LeftFirstPoint { get; set; }

        public Point RightFirstPoint { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int Dy { get; set; }
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
        public Point Location { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }

    public class SidePosition
    {
        public System.Drawing.Point Ban1 { get; set; }

        public System.Drawing.Point Ban2 { get; set; }

        public Point Ban3 { get; set; }

        public Point Pick1 { get; set; }

        public int Dx { get; set; }

        public int Dy { get; set; }

        public System.Drawing.Point[] HeroPathPoints { get; set; }

        public System.Drawing.Point HeroName1 { get; set; }
    }
}