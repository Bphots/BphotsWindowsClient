using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessor.Extensions
{
    /// <summary>
    /// Fast access to bitmap data
    /// </summary>
    public class FastBitmap : IDisposable
    {
        //=========================================================================
        #region Constructors
        /// <summary>
        /// Create a new <c>FastBitamp</c>
        /// </summary>
        public FastBitmap(Bitmap bitmap, int xx, int yy, int width, int height)
        {
            this.Bitmap = bitmap;
            XX = xx;
            YY = yy;
            Width = width;
            Height = height;
            Data = this.Bitmap.LockBits(new Rectangle(xx, yy, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            PixelSize = FindPixelSize();
            Stride = Data.Stride;
            Scan0 = Data.Scan0;
        }

        /// <summary>
        /// Create a new <c>FastBitamp</c>
        /// <param
        /// </summary>
        public FastBitmap(Bitmap bitmap) : this(bitmap, 0, 0, bitmap.Width, bitmap.Height)
        {
        }
        #endregion

        #region IDisposable
        /// <inheritdoc />
        public void Dispose()
        {
            Bitmap.UnlockBits(Data);
        }
        #endregion

        #region Public members
        /// <summary>
        /// The bitmap
        /// </summary>
        public Bitmap Bitmap;
        /// <summary>
        /// The data of bitmap
        /// </summary>
        public readonly BitmapData Data;
        /// <summary>
        /// The pixel size
        /// </summary>
        public readonly int PixelSize;
        /// <summary>
        /// The left of rectangle proccesed
        /// </summary>
        public readonly int XX;
        /// <summary>
        /// The top of rectangle proccesed
        /// </summary>
        public readonly int YY;
        /// <summary>
        /// The width of rectangle proccesed
        /// </summary>
        public readonly int Width;
        /// <summary>
        /// The height of rectangle proccesed
        /// </summary>
        public readonly int Height;
        /// <summary>
        /// the width of a single row
        /// </summary>
        public readonly int Stride;
        /// <summary>
        /// The first pixel location in rectangle proccesed
        /// </summary>
        public readonly IntPtr Scan0;

        #endregion

        #region private
        private int FindPixelSize()
        {
            if (Data.PixelFormat == PixelFormat.Format24bppRgb)
            {
                return 3;
            }
            if (Data.PixelFormat == PixelFormat.Format32bppArgb)
            {
                return 4;
            }
            return 4;
        }
        #endregion
    }
}

