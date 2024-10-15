using System;
using System.Collections.Generic;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="bool"/>.
/// </summary>
public static class BoolExtensions
{
    /// <summary>
    /// Converts a <see cref="bool"/> to a <see cref="bool"/>.
    /// </summary>
    public static bool AsBool(this bool b)
        => b;

    /// <summary>
    /// Converts a <see cref="bool"/> to an <see cref="int"/>.
    /// Returns 1 if true, 0 if false.
    /// </summary>
    public static int AsInt(this bool b)
        => Convert.ToInt32(b);

    /// <summary>
    /// Converts a <see cref="bool"/> to a <see cref="float"/>.
    /// Returns 1.0f if true, 0.0f if false.
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float AsFloat(this bool b)
        => Convert.ToSingle(b);

    /// <summary>
    /// Converts a <see cref="bool"/> to a <see cref="T:byte[]"/>.
    /// Returns a single byte with value 1 if true, 0 if false.
    /// </summary>
    public static byte[] AsBytes(this bool b)
        => BitConverter.GetBytes(b);

    /// <summary>
    /// Converts a <see cref="bool"/> to a <see cref="string"/>.
    /// Returns "True" if true, "False" if false.
    /// </summary>
    public static string AsString(this bool b)
        => b.ToString();

    /// <summary>
    /// Converts a <see cref="bool"/> to a <see cref="List{T}"/> of <see cref="string"/>.
    /// Returns a list with a single element containing the string representation of the bool.
    /// </summary>
    public static List<string> AsList(this bool b)
        => [b.ToString()];

    /// <summary>
    /// Converts a <see cref="bool"/> to a <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/>.
    /// Returns a dictionary with a single key-value pair containing the string representation of the bool
    /// as the key and an empty string as the value.
    /// </summary>
    public static Dictionary<string, string> AsDict(this bool b)
        => new() { { b.ToString(), "" } };
}
