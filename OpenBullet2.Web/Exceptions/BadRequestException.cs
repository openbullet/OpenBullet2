namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when the request has some malformed data.
/// </summary>
public class BadRequestException : ApiException
{
    /// <summary>
    /// Creates a <see cref="BadRequestException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public BadRequestException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
