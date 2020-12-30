using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace OpenBullet2.Helpers
{
    public static class ImageEditor
    {
        public static string ResizeBase64(string base64, int width, int height)
        {
            using Image image = Image.Load(Convert.FromBase64String(base64));
            
            image.Mutate(x => x
                .Resize(width, height));

            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
