namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when a resource already exists.
/// </summary>
public class ResourceAlreadyExistsException : ApiException
{
    /// <summary>
    /// Creates a <see cref="ResourceAlreadyExistsException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public ResourceAlreadyExistsException(string errorCode, string message)
        : base(errorCode, message)
    {
    }

    /// <summary>
    /// Creates a <see cref="ResourceAlreadyExistsException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="resource">The resource that already exists</param>
    /// <param name="path">The path where the resource was being saved</param>
    public ResourceAlreadyExistsException(string errorCode,
        string resource, string path) :
        base(errorCode, $"The resource '{resource}' already exists at {path}")
    {
    }
}
