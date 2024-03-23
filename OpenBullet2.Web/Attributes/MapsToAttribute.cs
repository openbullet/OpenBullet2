namespace OpenBullet2.Web.Attributes;

/// <summary>
/// Used to decorate a class that can be mapped to another class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class MapsToAttribute : Attribute
{
    /// <summary>
    /// Assigns the given destination type. If <paramref name="autoMap" />
    /// is <see langword="true" />, it will also register a default mapping
    /// in <see cref="AutoMapper.Mapper" />.
    /// </summary>
    public MapsToAttribute(Type destinationType, bool autoMap = true)
    {
        DestinationType = destinationType;
        AutoMap = autoMap;
    }

    /// <summary>
    /// The type that the decorated type maps to.
    /// </summary>
    public Type DestinationType { get; init; }

    /// <summary>
    /// Whether to register a default mapping in <see cref="AutoMapper.Mapper" />.
    /// </summary>
    public bool AutoMap { get; set; }
}
