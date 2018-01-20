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
            var is1080 = App.AppSetting.Position.Height == 1080;
            var frameRightBorderPoint = is1080 ? new Point(1907, 938) : new Point(3423, 1241);
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
            var is1080 = App.AppSetting.Position.Height == 1080;
            var appearanceFramePoint = is1080 ? new Point(70, 457) : new Point(81, 521);
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

            var skillFramePoint = is1080 ? new Point(77, 833) : new Point(121, 1111);
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

            var talentFramePoint = is1080 ? new Point(147, 319) : new Point(213, 425);
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
            var is1080 = App.AppSetting.Position.Height == 1080;
            var fullHorizontalPoint = is1080 ? new Point(1173, 248) : new Point(1960, 331);
            var isFull = true;
            var sampleColor = screenshotBitmap.GetPixel(fullHorizontalPoint.X, fullHorizontalPoint.Y);
            for (var x = fullHorizontalPoint.X + 1; x <= fullHorizontalPoint.X + 600; ++x)
            {
                var color = screenshotBitmap.GetPixel(x, fullHorizontalPoint.Y);
                if (Math.Abs(color.R - sampleColor.R) + Math.Abs(color.G - sampleColor.G) +
                    Math.Abs(color.B - sampleColor.B) <= 20) continue;

                isFull = false;
                break;
            }
            if (isFull)
                return ChatInfo.Full;

            var parialHorizontalPoint = is1080 ? new Point(1173, 632) : new Point(1960, 842);
            var isPartial = true;
            sampleColor = screenshotBitmap.GetPixel(parialHorizontalPoint.X, parialHorizontalPoint.Y);
            for (var x = parialHorizontalPoint.X + 1; x <= parialHorizontalPoint.X + 600; ++x)
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