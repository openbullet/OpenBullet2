namespace OpenBullet2.Web.Attributes;

/// <summary>
/// Used to decorate a class that can be mapped to another class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class MapsToAttribute : Attribute
{
    /// <summary>
    /// The type that the decorated type maps to.
    /// </summary>
    public Type DestinationType { get; init; }

    /// <summary>
    /// Assigns the given destination type.
    /// </summary>
    public MapsToAttribute(Type destinationType)
    {
        DestinationType = destinationType;
    }
}
