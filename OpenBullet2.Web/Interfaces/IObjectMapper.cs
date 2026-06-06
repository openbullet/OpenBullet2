namespace OpenBullet2.Web.Interfaces;

/// <summary>
/// Minimal object-mapping abstraction used by the web layer.
/// </summary>
public interface IObjectMapper
{
    /// <summary>
    /// Maps a source object to a new destination instance.
    /// </summary>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Applies a source object over an existing destination instance.
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    /// <summary>
    /// Maps a source object between runtime types.
    /// </summary>
    object? Map(object source, Type sourceType, Type destinationType);
}
