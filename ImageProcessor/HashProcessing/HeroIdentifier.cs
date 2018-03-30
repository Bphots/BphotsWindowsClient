using System;
using System.Drawing;

namespace ImageProcessor.HashProcessing
{
    public class HeroIdentifier
    {
        private static readonly AllHero Heroes = new AllHero();

        public static Tuple<int, double> Identify(Bitmap bitmap)
        {
            var img1 = new ImgHash(bitmap);
            return Heroes.GetAns(img1.GetHash());
        }
    }
}
