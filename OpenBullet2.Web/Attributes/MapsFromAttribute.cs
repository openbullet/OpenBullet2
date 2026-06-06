namespace OpenBullet2.Web.Attributes;

/// <summary>
/// Used to decorate a class that can be mapped from another class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class MapsFromAttribute : Attribute
{
    /// <summary>
    /// Assigns the given source type. If <paramref name="autoMap" />
    /// is <see langword="true" />, it will also register a default mapping
    /// in the web mapper configuration.
    /// </summary>
    public MapsFromAttribute(Type sourceType, bool autoMap = true)
    {
        SourceType = sourceType;
        AutoMap = autoMap;
    }

    /// <summary>
    /// The type that maps to the decorated type.
    /// </summary>
    public Type SourceType { get; init; }

    /// <summary>
    /// Whether to register a default mapping in the web mapper configuration.
    /// </summary>
    public bool AutoMap { get; set; }
}
