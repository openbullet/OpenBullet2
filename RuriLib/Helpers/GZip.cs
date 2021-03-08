using System.IO;
using System.IO.Compression;

namespace RuriLib.Helpers
{
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
            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }

            return mso.ToArray();
        }

        /// <summary>
        /// Unzips a GZipped content.
        /// </summary>
        public static byte[] Unzip(byte[] bytes)
        {
            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            return mso.ToArray();
        }

    }
}
