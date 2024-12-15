using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="T:List{string}"/>.
/// </summary>
public static class ListOfStringsExtensions
{
    /// <summary>
    /// Converts a list of strings to a boolean.
    /// Returns the first element of the list as a boolean,
    /// or false if the list is empty.
    /// </summary>
    public static bool AsBool(this List<string> list)
        => list.Count > 0 && list.First().AsBool();

    /// <summary>
    /// Converts a list of strings to an integer.
    /// Returns the first element of the list as an integer,
    /// or 0 if the list is empty.
    /// </summary>
    public static int AsInt(this List<string> list)
        => list.Count > 0 ? list.First().AsInt() : 0;

    /// <summary>
    /// Converts a list of strings to a float.
    /// Returns the first element of the list as a float,
    /// or 0.0f if the list is empty.
    /// </summary>
    public static float AsFloat(this List<string> list)
        => list.Count > 0 ? list.First().AsFloat() : 0.0f;

    /// <summary>
    /// Converts a list of strings to a byte array.
    /// Returns the first element of the list as a byte array,
    /// or an empty byte array if the list is empty.
    /// </summary>
    public static byte[] AsBytes(this List<string> list)
        => list.Count > 0 ? list.First().AsBytes() : [];

    /// <summary>
    /// Converts a list of strings to a string.
    /// Returns a string representation of the list.
    /// For example, ["value1", "value2"] becomes "[value1, value2]".
    /// </summary>
    public static string AsString(this List<string> list)
        => "[" + string.Join(", ", list) + "]";

    /// <summary>
    /// Converts a list of strings to a list of strings.
    /// Returns the list itself.
    /// </summary>
    public static List<string> AsList(this List<string> list)
        => list;

    /// <summary>
    /// Converts a list of strings to a dictionary of strings.
    /// Returns a dictionary with the list elements as keys and empty strings as values.
    /// </summary>
    public static Dictionary<string, string> AsDict(this List<string> list)
        => list.Count == list.Distinct().Count()
            ? list.ToDictionary(l => l, _ => "")
            : throw new InvalidCastException("When converting a list to dictionary, all keys must be unique");
}
