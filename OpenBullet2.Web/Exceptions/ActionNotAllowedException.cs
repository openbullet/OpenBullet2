namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when an action is not allowed.
/// </summary>
public class ActionNotAllowedException : ApiException
{
    /// <summary>
    /// Creates an <see cref="ActionNotAllowedException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public ActionNotAllowedException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
