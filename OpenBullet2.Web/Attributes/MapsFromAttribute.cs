namespace OpenBullet2.Web.Attributes;

/// <summary>
/// Used to decorate a class that can be mapped from another class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class MapsFromAttribute : Attribute
{
    /// <summary>
    /// The type that maps to the decorated type.
    /// </summary>
    public Type SourceType { get; init; }

    /// <summary>
    /// Assigns the given source type.
    /// </summary>
    public MapsFromAttribute(Type sourceType)
    {
        SourceType = sourceType;
    }
}
