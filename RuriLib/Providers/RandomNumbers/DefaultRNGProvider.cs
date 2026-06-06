using System;

namespace RuriLib.Providers.RandomNumbers;

/// <summary>
/// Default implementation of <see cref="IRNGProvider"/>.
/// </summary>
public class DefaultRNGProvider : IRNGProvider
{
    private readonly Random random = new();

    /// <summary>
    /// Creates a new <see cref="Random"/> seeded from an internal generator.
    /// </summary>
    /// <returns>A new random number generator.</returns>
    public Random GetNew()
        => new(random.Next(0, int.MaxValue));
}
