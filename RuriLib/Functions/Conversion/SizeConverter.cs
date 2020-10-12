using System;
using System.Globalization;

namespace RuriLib.Functions.Conversion
{
    public static class SizeConverter
    {
        /// <summary>
        /// Converts a long number of bytes into a readable string.
        /// </summary>
        /// <param name="byteCount">The number of bytes</param>
        /// <param name="outputBits">If true it will output e.g. Gbit instead of GB</param>
        /// <param name="binaryUnit">If true it will output e.g. GiB and Gibit instead of GB and Gbit</param>
        /// <param name="decimalPlaces">How many decimal places to print at most</param>
        /// <returns></returns>
        public static string ToReadableSize(long byteCount, bool outputBits = false, bool binaryUnit = false, int decimalPlaces = 2)
        {
            string[] suf;

            if (outputBits)
            {
                if (binaryUnit) suf = new string[] { "bit", "Kibit", "Mibit", "Gibit", "Tibit", "Pibit", "Eibit" };
                else suf = new string[] { "bit", "Kbit", "Mbit", "Gbit", "Tbit", "Pbit", "Ebit" };
            }
            else
            {
                if (binaryUnit) suf = new string[] { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };
                else suf = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            }

            if (byteCount == 0)
                return "0" + $" {suf[0]}";

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, binaryUnit ? 1024 : 1000)));
            double num = Math.Round(bytes / Math.Pow(binaryUnit ? 1024 : 1000, place), decimalPlaces + 1);
            return (Math.Sign(byteCount) * num * (outputBits ? 8 : 1))
                .ToString("0." + new string('0', decimalPlaces), CultureInfo.InvariantCulture) + $" {suf[place]}";
        }
    }
}
