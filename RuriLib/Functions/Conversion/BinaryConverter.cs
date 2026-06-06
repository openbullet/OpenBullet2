using RuriLib.Extensions;
using System;
using System.Linq;

namespace RuriLib.Functions.Conversion;

/// <summary>
/// Provides methods to convert binary strings to byte arrays and vice versa.
/// </summary>
public static class BinaryConverter
{
    /// <summary>
    /// Converts a <see cref="string"/> <paramref name="str"/> of zeroes and ones to an
    /// array of <see cref="byte"/>, optionally adding a padding to the left if one
    /// of the octets is incomplete.
    /// </summary>
    /// <param name="str">The binary string to decode.</param>
    /// <param name="addPadding">Whether incomplete octets should be left-padded with zeroes.</param>
    /// <returns>The decoded byte array.</returns>
    public static byte[] ToByteArray(string str, bool addPadding = true)
    {
        if (str.Contains(' '))
        {
            str = str.Replace(" ", "");
        }

        if (addPadding)
        {
            str = str.PadLeftToNearestMultiple(8);
        }

        return str.SplitInChunks(8, false)
            .Select(octet => Convert.ToByte(octet, 2))
            .ToArray();
    }

    /// <summary>
    /// Converts an array of <see cref="byte"/> to a string of ones and zeroes.
    /// </summary>
    /// <param name="bytes">The bytes to encode.</param>
    /// <returns>The binary string representation.</returns>
    public static string ToBinaryString(byte[] bytes)
        => string.Concat(bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
}
