using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace OpenBullet2.Helpers
{
    public static class ImageEditor
    {
        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="resolution">Sets the X and Y resolution if greater than 0</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height, int resolution = 0)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            if (resolution > 0)
                destImage.SetResolution(resolution, resolution);
            else
                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="base64Image">The image to resize as a base64 string.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="resolution">Sets the X and Y resolution if greater than 0</param>
        /// <returns>The resized image as a base64 string.</returns>
        public static string ResizeImageBase64(string base64Image, int width, int height, int resolution = 0)
        {
            var stream = new MemoryStream(Convert.FromBase64String(base64Image));
            var image = new Bitmap(stream);

            var resized = ResizeImage(image, width, height, resolution);
            
            MemoryStream ms = new MemoryStream();
            resized.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
