using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace OpenBullet2.Native.Utils;

public static class Images
{
    public static BitmapImage? Base64ToBitmapImage(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return null;
        }

        try
        {
            return BytesToBitmapImage(Convert.FromBase64String(base64));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public static BitmapImage? BytesToBitmapImage(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return null;
        }

        using var ms = new MemoryStream(bytes);
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();

            return image;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }
}
