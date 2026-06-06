using System;
using System.Collections.Generic;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="object"/>.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Converts an object of a supported type to a string.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// The type is not supported.
    /// </exception>
    public static string DynamicAsString(this object obj) => obj switch
    {
        bool x => x.AsString(),
        byte[] x => x.AsString(),
        long x => x.AsString(),
        int x => x.AsString(),
        double x => x.AsString(),
        float x => x.AsString(),
        string x => x.AsString(),
        List<string> x => x.AsString(),
        Dictionary<string, string> x => x.AsString(),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Converts an object of a supported type to an int.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// The type is not supported.
    /// </exception>
    public static int DynamicAsInt(this object obj) => obj switch
    {
        bool x => x.AsInt(),
        byte[] x => x.AsInt(),
        long x => x.AsInt(),
        int x => x.AsInt(),
        double x => x.AsInt(),
        float x => x.AsInt(),
        string x => x.AsInt(),
        List<string> x => x.AsInt(),
        // I have absolutely no idea why it wouldn't let me compile it with the extension method...
        Dictionary<string, string> x => DictionaryOfStringsExtensions.AsInt(x),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Converts an object of a supported type to a long.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// The type is not supported.
    /// </exception>
    public static long DynamicAsLong(this object obj) => obj switch
    {
        bool x => x.AsLong(),
        byte[] x => x.AsLong(),
        long x => x.AsLong(),
        int x => x.AsLong(),
        double x => x.AsLong(),
        float x => x.AsLong(),
        string x => x.AsLong(),
        List<string> x => x.AsLong(),
        Dictionary<string, string> x => DictionaryOfStringsExtensions.AsLong(x),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Converts an object of a supported type to a float.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// The type is not supported.
    /// </exception>
    public static float DynamicAsFloat(this object obj) => obj switch
    {
        bool x => x.AsFloat(),
        byte[] x => x.AsFloat(),
        long x => x.AsFloat(),
        int x => x.AsFloat(),
        double x => x.AsFloat(),
        float x => x.AsFloat(),
        string x => x.AsFloat(),
        List<string> x => x.AsFloat(),
        // I have absolutely no idea why it wouldn't let me compile it with the extension method...
        Dictionary<string, string> x => DictionaryOfStringsExtensions.AsFloat(x),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Converts an object of a supported type to a double.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// The type is not supported.
    /// </exception>
    public static double DynamicAsDouble(this object obj) => obj switch
    {
        bool x => x.AsDouble(),
        byte[] x => x.AsDouble(),
        long x => x.AsDouble(),
        int x => x.AsDouble(),
        double x => x.AsDouble(),
        float x => x.AsDouble(),
        string x => x.AsDouble(),
        List<string> x => x.AsDouble(),
        Dictionary<string, string> x => DictionaryOfStringsExtensions.AsDouble(x),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Converts an object of a supported type to a boolean.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// The type is not supported.
    /// </exception>
    public static bool DynamicAsBool(this object obj) => obj switch
    {
        bool x => x.AsBool(),
        byte[] x => x.AsBool(),
        long x => x.AsBool(),
        int x => x.AsBool(),
        double x => x.AsBool(),
        float x => x.AsBool(),
        string x => x.AsBool(),
        List<string> x => x.AsBool(),
        // I have absolutely no idea why it wouldn't let me compile it with the extension method...
        Dictionary<string, string> x => DictionaryOfStringsExtensions.AsBool(x),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Converts an object of a supported type to a list of strings.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// The type is not supported.
    /// </exception>
    public static List<string> DynamicAsList(this object obj) => obj switch
    {
        bool x => x.AsList(),
        byte[] x => x.AsList(),
        long x => x.AsList(),
        int x => x.AsList(),
        double x => x.AsList(),
        float x => x.AsList(),
        string x => x.AsList(),
        List<string> x => x.AsList(),
        Dictionary<string, string> x => x.AsList(),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Converts an object of a supported type to a dictionary of strings.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// The type is not supported.
    /// </exception>
    public static Dictionary<string, string> DynamicAsDict(this object obj) => obj switch
    {
        bool x => x.AsDict(),
        byte[] x => x.AsDict(),
        long x => x.AsDict(),
        int x => x.AsDict(),
        double x => x.AsDict(),
        float x => x.AsDict(),
        string x => x.AsDict(),
        List<string> x => x.AsDict(),
        Dictionary<string, string> x => x.AsDict(),
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Converts an object of a supported type to a byte array.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// The type is not supported.
    /// </exception>
    public static byte[] DynamicAsBytes(this object obj) => obj switch
    {
        bool x => x.AsBytes(),
        byte[] x => x.AsBytes(),
        long x => x.AsBytes(),
        int x => x.AsBytes(),
        double x => x.AsBytes(),
        float x => x.AsBytes(),
        string x => x.AsBytes(),
        List<string> x => x.AsBytes(),
        Dictionary<string, string> x => x.AsBytes(),
        _ => throw new NotImplementedException()
    };
}
