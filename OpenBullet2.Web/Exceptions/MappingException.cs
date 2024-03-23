namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a mapping operation fails.
/// </summary>
public class MappingException : Exception
{
    /// <summary>
    /// Creates a new <see cref="MappingException" /> with a message.
    /// </summary>
    public MappingException(string message) : base(message) { }
}
