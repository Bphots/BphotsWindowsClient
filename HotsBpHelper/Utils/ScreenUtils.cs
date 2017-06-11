using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace HotsBpHelper.Utils
{
    public static  class ScreenUtils
    {
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hDc, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        public const int LOGPIXELSX = 88;
        public const int LOGPIXELSY = 90;

        /// <summary>
        /// Transforms device independent units (1/96 of an inch)
        /// to pixels
        /// </summary>
        /// <param name="unitX">a device independent unit value X</param>
        /// <param name="unitY">a device independent unit value Y</param>
        /// <param name="pixelX">returns the X value in pixels</param>
        /// <param name="pixelY">returns the Y value in pixels</param>
        public static void TransformToPixels(double unitX,
            double unitY,
            out int pixelX,
            out int pixelY)
        {
            IntPtr hDc = GetDC(IntPtr.Zero);
            if (hDc != IntPtr.Zero)
            {
                int dpiX = GetDeviceCaps(hDc, LOGPIXELSX);
                int dpiY = GetDeviceCaps(hDc, LOGPIXELSY);

                ReleaseDC(IntPtr.Zero, hDc);

                pixelX = (int)(((double)dpiX / 96) * unitX);
                pixelY = (int)(((double)dpiY / 96) * unitY);
            }
            else
                throw new ArgumentNullException("Failed to get DC.");
        }

        public static Point ToPixelPoint(this Point unitPoint)
        {
            int pixelX, pixelY;
            TransformToPixels((int) unitPoint.X, (int) unitPoint.Y, out pixelX, out pixelY);
            return new Point(pixelX, pixelY);
        }

        /// <summary>
        /// Transforms device independent units (1/96 of an inch)
        /// from pixels
        /// </summary>
        /// <param name="pixelX">returns the X value in pixels</param>
        /// <param name="pixelY">returns the Y value in pixels</param>
        /// <param name="unitX">a device independent unit value X</param>
        /// <param name="unitY">a device independent unit value Y</param>
        public static void TransformFromPixels(int pixelX, int pixelY, out double unitX, out double unitY)
        {
            IntPtr hDc = GetDC(IntPtr.Zero);
            if (hDc != IntPtr.Zero)
            {
                int dpiX = GetDeviceCaps(hDc, LOGPIXELSX);
                int dpiY = GetDeviceCaps(hDc, LOGPIXELSY);

                ReleaseDC(IntPtr.Zero, hDc);

                unitX = pixelX * 96 / (double)dpiX;
                unitY = pixelY * 96 / (double)dpiY;
            }
            else
                throw new ArgumentNullException("Failed to get DC.");
        }

        public static Point ToUnitPoint(this Point pixelPoint)
        {
            double unitX, unitY;
            TransformFromPixels((int) pixelPoint.X, (int)pixelPoint.Y, out unitX, out unitY);
            return new Point(unitX, unitY);

        }
    }
}