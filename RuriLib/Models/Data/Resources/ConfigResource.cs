using System;
using System.Collections.Generic;

namespace RuriLib.Models.Data.Resources;

/// <summary>
/// Represents a named resource that can provide string values to a config.
/// </summary>
public abstract class ConfigResource
{
    /// <summary>
    /// Takes a single string from the resource.
    /// </summary>
    public virtual string TakeOne()
        => throw new NotImplementedException();

    /// <summary>
    /// Takes multiple elements strings the resource.
    /// </summary>
    /// <param name="amount">The number of elements to take.</param>
    /// <returns>The values taken from the resource.</returns>
    public virtual List<string> Take(int amount)
        => throw new NotImplementedException();
}
