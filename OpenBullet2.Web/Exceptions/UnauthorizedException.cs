namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when a user tries to
/// access a resource without being authorized.
/// </summary>
public class UnauthorizedException : ApiException
{
    /// <summary>
    /// Creates a <see cref="UnauthorizedException" /> with a message.
    /// </summary>
    public UnauthorizedException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
