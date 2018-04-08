using System;
using System.Drawing;
using HotsBpHelper.Utils;

namespace HotsBpHelper.HeroFinder
{
    public static class FrameFinder
    {
        public enum ChatInfo
        {
            None,
            Partial,
            Full
        }

        public static bool CheckIfInRightFrame(Bitmap screenshotBitmap)
        {
            if (App.AppSetting.Position.OverlapPoints == null || App.AppSetting.Position.OverlapPoints.AppearanceFramePoint == Point.Empty)
                return false;
            
            var frameRightBorderPoint = App.AppSetting.Position.OverlapPoints.FrameRightBorderPoint; 
            var hasFrame = true;
            var sampleColor = screenshotBitmap.GetPixel(frameRightBorderPoint.X, frameRightBorderPoint.Y);
            for (var y = frameRightBorderPoint.Y - 1; y >= frameRightBorderPoint.Y - 300; --y)
            {
                var color = screenshotBitmap.GetPixel(frameRightBorderPoint.X, y);
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) <= 25) continue;

                hasFrame = false;
                break;
            }

            return hasFrame;
        }

        public static bool CheckIfInLeftFrame(Bitmap screenshotBitmap)
        {
            if (App.AppSetting.Position.OverlapPoints == null || App.AppSetting.Position.OverlapPoints.AppearanceFramePoint == Point.Empty)
                return false;
            
            var appearanceFramePoint = App.AppSetting.Position.OverlapPoints.AppearanceFramePoint;
            var hasAppearanceFrame = true;
            var sampleColor = screenshotBitmap.GetPixel(appearanceFramePoint.X, appearanceFramePoint.Y);
            for (var x = appearanceFramePoint.X + 1; x <= appearanceFramePoint.X + 250; ++x)
            {
                var color = screenshotBitmap.GetPixel(x, appearanceFramePoint.Y);
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) <= 25) continue;

                hasAppearanceFrame = false;
                break;
            }
            if (hasAppearanceFrame)
                return true;

            var skillFramePoint = App.AppSetting.Position.OverlapPoints.SkillFramePoint;
            var hasSkilFrame = true;
            sampleColor = screenshotBitmap.GetPixel(skillFramePoint.X, skillFramePoint.Y);
            for (var x = skillFramePoint.X + 1; x <= skillFramePoint.X * 4; ++x)
            {
                var color = screenshotBitmap.GetPixel(x, skillFramePoint.Y);
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) <= 25) continue;

                hasSkilFrame = false;
                break;
            }
            if (hasSkilFrame)
                return true;

            var talentFramePoint = App.AppSetting.Position.OverlapPoints.TalentFramePoint;
            var hasTalentFrame = true;
            sampleColor = screenshotBitmap.GetPixel(talentFramePoint.X, talentFramePoint.Y);
            for (var x = talentFramePoint.X + 1; x <= talentFramePoint.X * 2; ++x)
            {
                var color = screenshotBitmap.GetPixel(x, talentFramePoint.Y);
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) <= 25) continue;

                hasTalentFrame = false;
                break;
            }
            if (hasTalentFrame)
                return true;

            return false;
        }

        public static ChatInfo CheckIfInChat(Bitmap screenshotBitmap)
        {
            if (App.AppSetting.Position.OverlapPoints == null || App.AppSetting.Position.OverlapPoints.AppearanceFramePoint == Point.Empty)
                return ChatInfo.None;

            var fullHorizontalPoint = App.AppSetting.Position.OverlapPoints.FullChatHorizontalPoint;
            var isFull = true;
            var sampleColor = screenshotBitmap.GetPixel(fullHorizontalPoint.X, fullHorizontalPoint.Y);
            for (var x = fullHorizontalPoint.X; x >= fullHorizontalPoint.X - 300; --x)
            {
                var color = screenshotBitmap.GetPixel(x, fullHorizontalPoint.Y);
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) <= 20) continue;

                isFull = false;
                break;
            }
            if (isFull)
                return ChatInfo.Full;

            var parialHorizontalPoint = App.AppSetting.Position.OverlapPoints.PartialChatlHorizontalPoint;
            var isPartial = true;
            sampleColor = screenshotBitmap.GetPixel(parialHorizontalPoint.X, parialHorizontalPoint.Y);
            for (var x = parialHorizontalPoint.X; x >= parialHorizontalPoint.X - 300; --x)
            {
                var color = screenshotBitmap.GetPixel(x, parialHorizontalPoint.Y);
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) <= 20) continue;

                isPartial = false;
                break;
            }
            if (isPartial)
                return ChatInfo.Partial;

            return ChatInfo.None;
        }
    }
}