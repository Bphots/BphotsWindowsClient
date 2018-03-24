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
        public Position()
        {}

        public Position(int width, int height)
        {
            Width = width;
            Height = height;
            BpHelperSize = new Size(425, 150);
            BpHelperPosition = new Point((int) (0.31*Height),
                0.852*Height + BpHelperSize.Height > Height ? (Height - BpHelperSize.Height) : (int)(0.852 * Height));
            MapSelectorPosition = new Point((int)(0.5 * Width), (int)(0.146 * Height));

            HeroWidth = (int)(0.08125 * Height);
            HeroHeight = (int) (0.0632*Height);

            Left = new SidePosition()
            {
                Ban1 = new Point((int) (0.45*Height), (int) (0.016*Height)),
                Ban2 = new Point((int) (0.45*Height), (int) (0.016*Height) + (int)(0.023 * Height) + (int) (0.015*Height)),
                Pick1 = new Point((int) (0.195*Height), (int) (0.132*Height)),
                Dx = (int) (0.0905*Height),
                Dy = (int) (0.1565*Height),
                HeroPathPoints =
                    new[]
                    {
                        new Point(1, 1), new Point(1, (int) (0.0185*Height)),
                        new Point(HeroWidth, HeroHeight),
                        new Point(HeroWidth, HeroHeight - (int) (0.0165*Height))
                    },
                HeroName1 = new Point((int) (0.013195*Height), (int) (0.172222*Height))
            };
            Right = new SidePosition()
            {
                Ban1 = new Point((int)(Width - 0.45 * Height), (int)(0.016 * Height)),
                Ban2 = new Point((int)(Width - 0.45 * Height), (int)(0.016 * Height) + (int)(0.023 * Height) + (int)(0.015 * Height)),
                Pick1 = new Point((int)(Width - 0.195 * Height), (int)(0.132 * Height)),
                Dx = (int)(-0.0905 * Height),
                Dy = (int)(0.1565 * Height),
                HeroPathPoints =
                    new[]
                    {
                        new Point(HeroWidth, 1), new Point(HeroWidth, 1 + (int) (0.0185*Height)),
                        new Point(1, HeroHeight),
                        new Point(1, HeroHeight - (int) (0.0165*Height))
                    },
                HeroName1 = new Point((int)(Width - 0.011195 * Height), (int)(0.172222 * Height))
            };
            MapPosition = new MapPosition()
            {
                Location = new Point((int) (Width/2 - 0.18*Height), 0),
                Width = (int) (0.36*Height),
                Height = (int) (0.03563*height)
            };
        }

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

        public System.Drawing.Point Pick1 { get; set; }

        public int Dx { get; set; }

        public int Dy { get; set; }

        public System.Drawing.Point[] HeroPathPoints { get; set; }

        public System.Drawing.Point HeroName1 { get; set; }
    }
}