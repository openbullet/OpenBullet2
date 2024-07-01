namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when a resource is in use and cannot
/// be modified or deleted.
/// </summary>
public class ResourceInUseException : ApiException
{
    /// <summary>
    /// Creates a <see cref="ResourceInUseException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public ResourceInUseException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
