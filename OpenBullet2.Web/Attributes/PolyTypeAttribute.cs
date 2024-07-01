namespace OpenBullet2.Web.Attributes;

/// <summary>
/// Used to decorate a class derived from another class
/// to discriminate the actual class in (de)serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PolyTypeAttribute : Attribute
{
    /// <summary>
    /// Assigns the given poly type discriminator.
    /// </summary>
    public PolyTypeAttribute(string polyType)
    {
        PolyType = polyType;
    }

    /// <summary>
    /// The polymorphic type discriminator.
    /// </summary>
    public string PolyType { get; init; }

    /// <summary>
    /// Gets the <see cref="PolyTypeAttribute" /> of a class
    /// or null if it was not assigned.
    /// </summary>
    public static PolyTypeAttribute? FromType(Type type)
    {
        var attributes = type.GetCustomAttributes(true);
        return (PolyTypeAttribute?)attributes
            .SingleOrDefault(a => a is PolyTypeAttribute);
    }
}
