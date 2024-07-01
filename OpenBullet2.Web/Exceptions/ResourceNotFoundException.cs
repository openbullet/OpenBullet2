namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when a resource was not found.
/// </summary>
public class ResourceNotFoundException : ApiException
{
    /// <summary>
    /// Creates a <see cref="ResourceNotFoundException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public ResourceNotFoundException(string errorCode, string message)
        : base(errorCode, message)
    {
    }

    /// <summary>
    /// Creates a <see cref="ResourceNotFoundException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="resource">The resource that was not found</param>
    /// <param name="path">The path from which the resource was being read</param>
    public ResourceNotFoundException(string errorCode,
        string resource, string path) :
        base(errorCode, $"Could not find the resource '{resource}' at {path}")
    {
    }
}
