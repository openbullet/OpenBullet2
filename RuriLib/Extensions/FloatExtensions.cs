using System;
using System.Collections.Generic;
using System.Globalization;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="float"/> type.
/// </summary>
public static class FloatExtensions
{
    /// <summary>
    /// Converts a <see cref="float"/> to a <see cref="bool"/>.
    /// Returns true if the float is equal to 1, false otherwise.
    /// </summary>
    public static bool AsBool(this float f)
        => Math.Abs(f - 1f) < float.Epsilon;

    /// <summary>
    /// Converts a <see cref="float"/> to an <see cref="int"/>.
    /// Returns the integer part of the float.
    /// </summary>
    public static int AsInt(this float f)
        => (int)f;

    /// <summary>
    /// Converts a <see cref="float"/> to a <see cref="float"/>.
    /// </summary>
    public static float AsFloat(this float f)
        => f;

    /// <summary>
    /// Converts a <see cref="float"/> to a <see cref="T:byte[]"/>.
    /// Returns the byte representation of the float.
    /// </summary>
    public static byte[] AsBytes(this float f)
        => BitConverter.GetBytes(f);

    /// <summary>
    /// Converts a <see cref="float"/> to a <see cref="string"/>.
    /// Returns the string representation of the float with all the decimal places.
    /// </summary>
    public static string AsString(this float f)
    {
        var formatted = f.ToString("0.#################################################################################", CultureInfo.InvariantCulture);
        return formatted.EndsWith('.') ? formatted.Trim('.') : formatted;
    }

    /// <summary>
    /// Converts a <see cref="float"/> to a <see cref="List{T}"/> of <see cref="string"/>.
    /// Returns a list with a single element containing the string representation of the float.
    /// </summary>
    public static List<string> AsList(this float f)
        => [f.AsString()];

    /// <summary>
    /// Converts a <see cref="float"/> to a <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/>.
    /// Returns a dictionary with a single key-value pair containing the string representation of the float
    /// as the key and an empty string as the value.
    /// </summary>
    public static Dictionary<string, string> AsDict(this float f)
        => new() { { f.AsString(), "" } };
}
