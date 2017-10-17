using System.Drawing;

namespace HotsBpHelper.Utils
{
    public interface IImageUtil
    {
        Bitmap CaptureScreen();

        Bitmap CaptureArea(Bitmap bmp, Rectangle rect, Point[] clipPoints);

        Bitmap CopBitmap(Bitmap bmp, Rectangle cropArea);

        Bitmap RotateImage(Bitmap bmp, float angle);

        bool IsSimilarColor(Color color1, Color color2);

        Bitmap CaptureScreen(int x1, int y1, int x2, int y2);

        Bitmap CaptureWindow(int hwnd);
    }
}