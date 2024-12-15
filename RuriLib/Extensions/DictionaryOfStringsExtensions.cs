using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="T:Dictionary{string, string}"/>.
/// </summary>
public static class DictionaryOfStringsExtensions
{
    /// <summary>
    /// Not implemented. Throws an <see cref="InvalidCastException"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Always thrown.
    /// </exception>
    public static bool AsBool(Dictionary<string, string> _)
        => throw new InvalidCastException("Cannot convert a dictionary of strings to a bool");

    /// <summary>
    /// Not implemented. Throws an <see cref="InvalidCastException"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Always thrown.
    /// </exception>
    public static int AsInt(Dictionary<string, string> _)
        => throw new InvalidCastException("Cannot convert a dictionary of strings to an int");

    /// <summary>
    /// Not implemented. Throws an <see cref="InvalidCastException"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Always thrown.
    /// </exception>
    public static float AsFloat(Dictionary<string, string> _)
        => throw new InvalidCastException("Cannot convert a dictionary of strings to a float");

    /// <summary>
    /// Not implemented. Throws an <see cref="InvalidCastException"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Always thrown.
    /// </exception>
    public static byte[] AsBytes(this Dictionary<string, string> _)
        => throw new InvalidCastException("Cannot convert a dictionary of strings to a byte array");

    /// <summary>
    /// Converts a <see cref="T:Dictionary{string, string}"/> to a <see cref="string"/>.
    /// Returns a string representation of the dictionary.
    /// For example, {"key1", "value1"}, {"key2", "value2"} becomes "{(key1, value1), (key2, value2)}".
    /// </summary>
    public static string AsString(this Dictionary<string, string> dict)
        => "{" + string.Join(", ", dict.AsList().Select(s => $"({s})")) + "}";

    /// <summary>
    /// Converts a <see cref="T:Dictionary{string, string}"/> to a <see cref="List{T}"/> of <see cref="string"/>.
    /// Returns a list of strings where each element is a comma-separated key-value pair.
    /// For example, {"key1", "value1"}, {"key2", "value2"} becomes ["key1, value1", "key2, value2"].
    /// </summary>
    public static List<string> AsList(this Dictionary<string, string> dict)
        => dict.Select(kvp => $"{kvp.Key}, {kvp.Value}").ToList();

    /// <summary>
    /// Converts a <see cref="T:Dictionary{string, string}"/> to a <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/>.
    /// Returns the dictionary itself.
    /// </summary>
    public static Dictionary<string, string> AsDict(this Dictionary<string, string> dict)
        => dict;
}
