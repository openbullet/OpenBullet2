using System;
using System.Globalization;

namespace RuriLib.Functions.Conversion;

/// <summary>
/// Provides helpers to convert byte counts into readable sizes.
/// </summary>
public static class SizeConverter
{
    /// <summary>
    /// Converts a long number of bytes into a readable string.
    /// </summary>
    /// <param name="byteCount">The number of bytes</param>
    /// <param name="outputBits">If true it will output e.g. Gbit instead of GB</param>
    /// <param name="binaryUnit">If true it will output e.g. GiB and Gibit instead of GB and Gbit</param>
    /// <param name="decimalPlaces">How many decimal places to print at most</param>
    /// <returns>The readable size string.</returns>
    public static string ToReadableSize(long byteCount, bool outputBits = false, bool binaryUnit = false, int decimalPlaces = 2)
    {
        var suffixes = outputBits
            ? binaryUnit
                ? new[] { "bit", "Kibit", "Mibit", "Gibit", "Tibit", "Pibit", "Eibit" }
                : new[] { "bit", "Kbit", "Mbit", "Gbit", "Tbit", "Pbit", "Ebit" }
            : binaryUnit
                ? new[] { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" }
                : new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        if (byteCount == 0)
        {
            return $"0 {suffixes[0]}";
        }

        var bytes = Math.Abs(byteCount);
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, binaryUnit ? 1024 : 1000)));
        var num = Math.Round(bytes / Math.Pow(binaryUnit ? 1024 : 1000, place), decimalPlaces + 1);

        return (Math.Sign(byteCount) * num * (outputBits ? 8 : 1))
            .ToString($"0.{new string('0', decimalPlaces)}", CultureInfo.InvariantCulture) + $" {suffixes[place]}";
    }
}
