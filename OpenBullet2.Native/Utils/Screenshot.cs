using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace OpenBullet2.Native.Utils
{
    public static class Screenshot
    {
        public static void Take(int width, int height, int top, int left)
        {
            var bitmap = CopyScreen(width, height, top, left);

            // Copy it to the clipboard
            Clipboard.SetImage(bitmap);

            // Save it to the screenshot.jpg file
            GetBitmap(bitmap).Save("screenshot.jpg", ImageFormat.Jpeg);
        }

        private static BitmapSource CopyScreen(int width, int height, int top, int left)
        {
            using var screenBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using var bmpGraphics = Graphics.FromImage(screenBmp);
            bmpGraphics.CopyFromScreen(left, top, 0, 0, screenBmp.Size);

            return Imaging.CreateBitmapSourceFromHBitmap(
                screenBmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private static Bitmap GetBitmap(BitmapSource source)
        {
            var bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              PixelFormat.Format32bppPArgb);

            var data = bmp.LockBits(
              new Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              PixelFormat.Format32bppPArgb);

            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);

            bmp.UnlockBits(data);
            return bmp;
        }
    }
}
