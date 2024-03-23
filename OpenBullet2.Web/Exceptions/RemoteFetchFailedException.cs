namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when a remote resource could
/// not be fetched.
/// </summary>
public class RemoteFetchFailedException : ApiException
{
    /// <summary>
    /// Creates a <see cref="RemoteFetchFailedException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public RemoteFetchFailedException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
