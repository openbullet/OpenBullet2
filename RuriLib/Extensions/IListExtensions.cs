using System;
using System.Collections.Generic;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IList{T}"/>.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Randomizes the elements in a list.
    /// </summary>
    /// <param name="list">The list to shuffle</param>
    /// <param name="rng">The random number generator</param>
    public static void Shuffle<T>(this IList<T> list, Random? rng = null)
    {
        var rand = rng ?? new Random();
        var n = list.Count;
        
        while (n > 1)
        {
            n--;
            var k = rand.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
