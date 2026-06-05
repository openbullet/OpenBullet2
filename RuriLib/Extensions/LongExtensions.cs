using System;
using System.Collections.Generic;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="long"/> type.
/// </summary>
public static class LongExtensions
{
    /// <summary>
    /// Converts a <see cref="long"/> to a <see cref="bool"/>.
    /// Returns true if the integer is different from 0, false otherwise.
    /// </summary>
    public static bool AsBool(this long i)
        => i != 0;

    /// <summary>
    /// Converts a <see cref="long"/> to an <see cref="int"/>.
    /// </summary>
    public static int AsInt(this long i)
        => Convert.ToInt32(i);

    /// <summary>
    /// Converts a <see cref="long"/> to a <see cref="long"/>.
    /// </summary>
    public static long AsLong(this long i)
        => i;

    /// <summary>
    /// Converts a <see cref="long"/> to a <see cref="float"/>.
    /// </summary>
    public static float AsFloat(this long i)
        => Convert.ToSingle(i);

    /// <summary>
    /// Converts a <see cref="long"/> to a <see cref="double"/>.
    /// </summary>
    public static double AsDouble(this long i)
        => i;

    /// <summary>
    /// Converts a <see cref="long"/> to an array of <see cref="byte"/>.
    /// Returns the byte representation of the integer.
    /// </summary>
    public static byte[] AsBytes(this long i)
        => BitConverter.GetBytes(i);

    /// <summary>
    /// Converts a <see cref="long"/> to a <see cref="string"/>.
    /// Returns the string representation of the integer.
    /// </summary>
    public static string AsString(this long i)
        => i.ToString();

    /// <summary>
    /// Converts a <see cref="long"/> to a <see cref="List{T}"/> of <see cref="string"/>.
    /// </summary>
    public static List<string> AsList(this long i)
        => [i.AsString()];

    /// <summary>
    /// Converts a <see cref="long"/> to a <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/>.
    /// </summary>
    public static Dictionary<string, string> AsDict(this long i)
        => new() { { i.AsString(), "" } };
}
