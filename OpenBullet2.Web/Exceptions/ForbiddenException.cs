namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when a user tries to
/// access a resource they are not allowed to.
/// </summary>
public class ForbiddenException : ApiException
{
    /// <summary>
    /// Creates a <see cref="ForbiddenException" /> with a message.
    /// </summary>
    public ForbiddenException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
