using System.IO;
using System.IO.Compression;

namespace RuriLib.Helpers;

/*
 * Taken from https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp
 * */

/// <summary>
/// GZip utilities class.
/// </summary>
public static class GZip
{
    /// <summary>
    /// GZips a content.
    /// </summary>
    public static byte[] Zip(byte[] bytes)
    {
        using var input = new MemoryStream(bytes);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress))
        {
            input.CopyTo(gzip);
        }

        return output.ToArray();
    }

    /// <summary>
    /// Unzips a GZipped content.
    /// </summary>
    public static byte[] Unzip(byte[] bytes)
    {
        using var input = new MemoryStream(bytes);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(input, CompressionMode.Decompress))
        {
            gzip.CopyTo(output);
        }

        return output.ToArray();
    }
}
