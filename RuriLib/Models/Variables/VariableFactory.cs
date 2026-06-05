using System;
using System.Collections.Generic;

namespace RuriLib.Models.Variables;

/// <summary>
/// Creates <see cref="Variable"/> instances from runtime objects.
/// </summary>
public class VariableFactory
{
    /// <summary>
    /// Wraps an object into the matching <see cref="Variable"/> implementation.
    /// </summary>
    /// <param name="obj">The object to wrap.</param>
    /// <returns>The matching variable wrapper.</returns>
    public static Variable FromObject(object obj) => obj switch
    {
        bool x => new BoolVariable(x),
        byte[] x => new ByteArrayVariable(x),
        Dictionary<string, string> x => new DictionaryOfStringsVariable(x),
        double x => new FloatVariable(x),
        float x => new FloatVariable(x),
        long x => new IntVariable(x),
        int x => new IntVariable(x),
        List<string> x => new ListOfStringsVariable(x),
        string x => new StringVariable(x),
        _ => throw new NotSupportedException("Type: " + (obj?.GetType().FullName ?? "null"))
    };
}
