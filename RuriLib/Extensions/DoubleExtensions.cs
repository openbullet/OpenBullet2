using System;
using System.Collections.Generic;
using System.Globalization;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="double"/> type.
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    /// Converts a <see cref="double"/> to a <see cref="bool"/>.
    /// Returns true if the double is equal to 1, false otherwise.
    /// </summary>
    public static bool AsBool(this double d)
        => Math.Abs(d - 1d) < double.Epsilon;

    /// <summary>
    /// Converts a <see cref="double"/> to an <see cref="int"/>.
    /// </summary>
    public static int AsInt(this double d)
        => Convert.ToInt32(d);

    /// <summary>
    /// Converts a <see cref="double"/> to an <see cref="long"/>.
    /// </summary>
    public static long AsLong(this double d)
        => Convert.ToInt64(d);

    /// <summary>
    /// Converts a <see cref="double"/> to a <see cref="float"/>.
    /// </summary>
    public static float AsFloat(this double d)
        => Convert.ToSingle(d);

    /// <summary>
    /// Converts a <see cref="double"/> to a <see cref="double"/>.
    /// </summary>
    public static double AsDouble(this double d)
        => d;

    /// <summary>
    /// Converts a <see cref="double"/> to a <see cref="T:byte[]"/>.
    /// Returns the byte representation of the double.
    /// </summary>
    public static byte[] AsBytes(this double d)
        => BitConverter.GetBytes(d);

    /// <summary>
    /// Converts a <see cref="double"/> to a <see cref="string"/>.
    /// Returns the string representation of the double with all the decimal places.
    /// </summary>
    public static string AsString(this double d)
    {
        var formatted = d.ToString("0.#################################################################################", CultureInfo.InvariantCulture);
        return formatted.EndsWith('.') ? formatted.Trim('.') : formatted;
    }

    /// <summary>
    /// Converts a <see cref="double"/> to a <see cref="List{T}"/> of <see cref="string"/>.
    /// </summary>
    public static List<string> AsList(this double d)
        => [d.AsString()];

    /// <summary>
    /// Converts a <see cref="double"/> to a <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/>.
    /// </summary>
    public static Dictionary<string, string> AsDict(this double d)
        => new() { { d.AsString(), "" } };

    /// <summary>
    /// Converts a <see cref="double"/> to a <see cref="float"/>.
    /// </summary>
    public static float ToSingle(this double d)
        => d.AsFloat();

    /// <summary>
    /// Converts a <see cref="double"/> to an <see cref="int"/>.
    /// </summary>
    public static int ToInt(this double d)
        => d.AsInt();

    /// <summary>
    /// Converts a <see cref="double"/> to an <see cref="long"/>.
    /// </summary>
    public static long ToLong(this double d)
        => d.AsLong();
}
