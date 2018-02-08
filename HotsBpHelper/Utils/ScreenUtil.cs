using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace HotsBpHelper.Utils
{
    public enum DeviceCap
    {
        /// <summary>
        /// Device driver version
        /// </summary>
        DRIVERVERSION = 0,
        /// <summary>
        /// Device classification
        /// </summary>
        TECHNOLOGY = 2,
        /// <summary>
        /// Horizontal size in millimeters
        /// </summary>
        HORZSIZE = 4,
        /// <summary>
        /// Vertical size in millimeters
        /// </summary>
        VERTSIZE = 6,
        /// <summary>
        /// Horizontal width in pixels
        /// </summary>
        HORZRES = 8,
        /// <summary>
        /// Vertical height in pixels
        /// </summary>
        VERTRES = 10,
        /// <summary>
        /// Number of bits per pixel
        /// </summary>
        BITSPIXEL = 12,
        /// <summary>
        /// Number of planes
        /// </summary>
        PLANES = 14,
        /// <summary>
        /// Number of brushes the device has
        /// </summary>
        NUMBRUSHES = 16,
        /// <summary>
        /// Number of pens the device has
        /// </summary>
        NUMPENS = 18,
        /// <summary>
        /// Number of markers the device has
        /// </summary>
        NUMMARKERS = 20,
        /// <summary>
        /// Number of fonts the device has
        /// </summary>
        NUMFONTS = 22,
        /// <summary>
        /// Number of colors the device supports
        /// </summary>
        NUMCOLORS = 24,
        /// <summary>
        /// Size required for device descriptor
        /// </summary>
        PDEVICESIZE = 26,
        /// <summary>
        /// Curve capabilities
        /// </summary>
        CURVECAPS = 28,
        /// <summary>
        /// Line capabilities
        /// </summary>
        LINECAPS = 30,
        /// <summary>
        /// Polygonal capabilities
        /// </summary>
        POLYGONALCAPS = 32,
        /// <summary>
        /// Text capabilities
        /// </summary>
        TEXTCAPS = 34,
        /// <summary>
        /// Clipping capabilities
        /// </summary>
        CLIPCAPS = 36,
        /// <summary>
        /// Bitblt capabilities
        /// </summary>
        RASTERCAPS = 38,
        /// <summary>
        /// Length of the X leg
        /// </summary>
        ASPECTX = 40,
        /// <summary>
        /// Length of the Y leg
        /// </summary>
        ASPECTY = 42,
        /// <summary>
        /// Length of the hypotenuse
        /// </summary>
        ASPECTXY = 44,
        /// <summary>
        /// Shading and Blending caps
        /// </summary>
        SHADEBLENDCAPS = 45,

        /// <summary>
        /// Logical pixels inch in X
        /// </summary>
        LOGPIXELSX = 88,
        /// <summary>
        /// Logical pixels inch in Y
        /// </summary>
        LOGPIXELSY = 90,

        /// <summary>
        /// Number of entries in physical palette
        /// </summary>
        SIZEPALETTE = 104,
        /// <summary>
        /// Number of reserved entries in palette
        /// </summary>
        NUMRESERVED = 106,
        /// <summary>
        /// Actual color resolution
        /// </summary>
        COLORRES = 108,

        // Printing related DeviceCaps. These replace the appropriate Escapes
        /// <summary>
        /// Physical Width in device units
        /// </summary>
        PHYSICALWIDTH = 110,
        /// <summary>
        /// Physical Height in device units
        /// </summary>
        PHYSICALHEIGHT = 111,
        /// <summary>
        /// Physical Printable Area x margin
        /// </summary>
        PHYSICALOFFSETX = 112,
        /// <summary>
        /// Physical Printable Area y margin
        /// </summary>
        PHYSICALOFFSETY = 113,
        /// <summary>
        /// Scaling factor x
        /// </summary>
        SCALINGFACTORX = 114,
        /// <summary>
        /// Scaling factor y
        /// </summary>
        SCALINGFACTORY = 115,

        /// <summary>
        /// Current vertical refresh rate of the display device (for displays only) in Hz
        /// </summary>
        VREFRESH = 116,
        /// <summary>
        /// Vertical height of entire desktop in pixels
        /// </summary>
        DESKTOPVERTRES = 117,
        /// <summary>
        /// Horizontal width of entire desktop in pixels
        /// </summary>
        DESKTOPHORZRES = 118,
        /// <summary>
        /// Preferred blt alignment
        /// </summary>
        BLTALIGNMENT = 119
    }
    public static class ScreenUtil
    {

        #region Get System DPI
        
        /// <summary>Determines the current screen resolution in DPI.</summary>
        /// <returns>Point.X is the X DPI, Point.Y is the Y DPI.</returns>
        public static Hardcodet.Wpf.TaskbarNotification.Interop.Point GetSystemDpi()
        {
            Hardcodet.Wpf.TaskbarNotification.Interop.Point result = new Hardcodet.Wpf.TaskbarNotification.Interop.Point();

            IntPtr hDC = GetDC(IntPtr.Zero);

            result.X = GetDeviceCaps(hDC, (int)DeviceCap.LOGPIXELSX);
            result.Y = GetDeviceCaps(hDC, (int)DeviceCap.LOGPIXELSY);

            ReleaseDC(IntPtr.Zero, hDC);

            return result;
        }
        
        #endregion

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hDc, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

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
                int dpiX = GetDeviceCaps(hDc, (int)DeviceCap.LOGPIXELSX);
                int dpiY = GetDeviceCaps(hDc, (int)DeviceCap.LOGPIXELSY);

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

            TransformToPixels(unitPoint.X, unitPoint.Y, out pixelX, out pixelY);
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
        public static void TransformFromPixels(int pixelX, int pixelY, out int unitX, out int unitY)
        {
            IntPtr hDc = GetDC(IntPtr.Zero);
            if (hDc != IntPtr.Zero)
            {
                int dpiX = GetDeviceCaps(hDc, (int)DeviceCap.LOGPIXELSX);
                int dpiY = GetDeviceCaps(hDc, (int)DeviceCap.LOGPIXELSY);

                ReleaseDC(IntPtr.Zero, hDc);
                
                unitX = (int) (pixelX * 96 / (double)dpiX);
                unitY = (int) (pixelY * 96 / (double)dpiY);
            }
            else
                throw new ArgumentNullException("Failed to get DC.");
        }

        public static Point ToUnitPoint(this Point pixelPoint)
        {
            int unitX, unitY;
            TransformFromPixels(pixelPoint.X, pixelPoint.Y, out unitX, out unitY);
            return new Point(unitX, unitY);

        }
        public static Size ToUnitSize(this Size pixelSize)
        {
            int unitWidth, unitHeight;
            TransformFromPixels(pixelSize.Width, pixelSize.Height, out unitWidth, out unitHeight);
            return new Size(unitWidth, unitHeight);

        }

        public static Size GetScreenResolution()
        {
            IntPtr hDc = GetDC(IntPtr.Zero);
            if (hDc != IntPtr.Zero)
            {
                int width = GetDeviceCaps(hDc, (int)DeviceCap.HORZRES);
                int height = GetDeviceCaps(hDc, (int)DeviceCap.VERTRES);

                ReleaseDC(IntPtr.Zero, hDc);

                return new Size(width, height);
            }
            else
                throw new ArgumentNullException("Failed to get DC.");
        }
    }
}