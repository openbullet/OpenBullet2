using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="T:byte[]"/>.
/// </summary>
public static class ByteArrayExtensions
{
    /// <summary>
    /// Converts a <see cref="T:byte[]"/> to a <see cref="bool"/>.
    /// Returns true if the first byte is not 0, false otherwise.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When the byte array is empty.
    /// </exception>
    public static bool AsBool(this byte[] bytes)
        => BitConverter.ToBoolean(bytes, 0);

    /// <summary>
    /// Converts a <see cref="T:byte[]"/> to an <see cref="int"/>.
    /// Returns the integer representation of the first 4 bytes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When the byte array cannot be converted to an integer.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// When the byte array is too short to be converted to an integer.
    /// </exception>
    public static int AsInt(this byte[] bytes)
        => BitConverter.ToInt32(bytes, 0);

    /// <summary>
    /// Converts a <see cref="T:byte[]"/> to a <see cref="float"/>.
    /// Returns the float representation of the first 4 bytes.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When the byte array cannot be converted to a float.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// When the byte array is too short to be converted to a float.
    /// </exception>
    public static float AsFloat(this byte[] bytes)
        => BitConverter.ToSingle(bytes, 0);

    /// <summary>
    /// Converts a <see cref="T:byte[]"/> to a <see cref="T:byte[]"/>.
    /// Returns the byte array itself.
    /// </summary>
    public static byte[] AsBytes(this byte[] bytes)
        => bytes;

    /// <summary>
    /// Converts a <see cref="T:byte[]"/> to a <see cref="string"/>.
    /// Returns the UTF8 string representation of the byte array.
    /// </summary>
    public static string AsString(this byte[] bytes)
        => Encoding.UTF8.GetString(bytes);

    /// <summary>
    /// Converts a <see cref="T:byte[]"/> to a <see cref="List{T}"/> of <see cref="string"/>.
    /// Returns a list with a single element containing the string representation of the byte array.
    /// </summary>
    public static List<string> AsList(this byte[] bytes) => [bytes.ToString()!];

    /// <summary>
    /// Converts a <see cref="T:byte[]"/> to a <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/>.
    /// Returns a dictionary with a single key-value pair containing the string representation of the byte array
    /// as the key and an empty string as the value.
    /// </summary>
    public static Dictionary<string, string> AsDict(this byte[] bytes)
        => new() { { bytes.ToString()!, "" } };
}
