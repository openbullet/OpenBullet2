using System.Threading.Tasks;

namespace RuriLib.Models.Hits;

/// <summary>
/// Stores hits in an output sink.
/// </summary>
public interface IHitOutput
{
    /// <summary>
    /// Stores a hit.
    /// </summary>
    /// <param name="hit">The hit to store.</param>
    /// <returns>A task that completes when the hit has been stored.</returns>
    Task Store(Hit hit);
}
