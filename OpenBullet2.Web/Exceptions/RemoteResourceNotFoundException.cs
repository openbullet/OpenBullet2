namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when a remote resource was not found.
/// </summary>
public class RemoteResourceNotFoundException : ApiException
{
    /// <summary>
    /// Creates a <see cref="RemoteResourceNotFoundException"/>.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public RemoteResourceNotFoundException(ErrorCode errorCode, string message) 
        : base(errorCode, message)
    {

    }

    /// <summary>
    /// Creates a <see cref="RemoteResourceNotFoundException"/>.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="resource">The resource that was not found</param>
    /// <param name="uri">The URI from which the resource was being fetched</param>
    public RemoteResourceNotFoundException(ErrorCode errorCode,
        string resource, string uri) : 
        base(errorCode, $"Could not find the remote resource '{resource}' at {uri}")
    {

    }
}
