using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace OpenBullet2.Core.Helpers
{
    public static class ImageEditor
    {
        public static byte[] ToCompatibleFormat(byte[] bytes)
        {
            // ICO magic numbers
            if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0x01 && bytes[3] == 0x00)
            {
                using var ms = new MemoryStream(bytes);
                var icon = new Icon(ms);
                var bitmap = icon.ToBitmap();

                using var ms2 = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }

            return bytes;
        }

        public static string ResizeBase64(string base64, int width, int height)
        {
            using var image = SixLabors.ImageSharp.Image.Load(Convert.FromBase64String(base64));

            image.Mutate(x => x
                .Resize(width, height));

            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
