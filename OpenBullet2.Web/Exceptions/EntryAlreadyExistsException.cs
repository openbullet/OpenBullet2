namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when an entry already exists.
/// </summary>
public class EntryAlreadyExistsException : ApiException
{
    /// <summary>
    /// Creates a <see cref="EntryAlreadyExistsException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public EntryAlreadyExistsException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
