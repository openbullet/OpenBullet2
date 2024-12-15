using System;
using System.Collections.Generic;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="int"/> type.
/// </summary>
public static class IntExtensions
{
    /// <summary>
    /// Converts an <see cref="int"/> to a <see cref="bool"/>.
    /// Returns true if the integer is different from 0, false otherwise.
    /// </summary>
    public static bool AsBool(this int i)
        => i != 0;

    /// <summary>
    /// Converts an <see cref="int"/> to an <see cref="int"/>.
    /// </summary>
    public static int AsInt(this int i)
        => i;

    /// <summary>
    /// Converts an <see cref="int"/> to a <see cref="float"/>.
    /// </summary>
    public static float AsFloat(this int i)
        => Convert.ToSingle(i);

    /// <summary>
    /// Converts an <see cref="int"/> to an array of <see cref="byte"/>.
    /// Returns the byte representation of the integer.
    /// </summary>
    public static byte[] AsBytes(this int i)
        => BitConverter.GetBytes(i);

    /// <summary>
    /// Converts an <see cref="int"/> to a <see cref="string"/>.
    /// Returns the string representation of the integer.
    /// </summary>
    public static string AsString(this int i)
        => i.ToString();

    /// <summary>
    /// Converts an <see cref="int"/> to a <see cref="List{T}"/> of <see cref="string"/>.
    /// Returns a list with a single element containing the string representation of the integer.
    /// </summary>
    public static List<string> AsList(this int i)
        => [i.AsString()];

    /// <summary>
    /// Converts an <see cref="int"/> to a <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/>.
    /// Returns a dictionary with a single key-value pair containing the string representation of the integer
    /// as the key and an empty string as the value.
    /// </summary>
    public static Dictionary<string, string> AsDict(this int i)
        => new() { { i.AsString(), "" } };
}
