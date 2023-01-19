namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// A generic API exception.
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// The error code.
    /// </summary>
    public ErrorCode ErrorCode { get; set; }

    /// <summary>
    /// Creates an <see cref="ApiException"/> given an 
    /// <paramref name="errorCode"/> and a <paramref name="message"/>.
    /// </summary>
    public ApiException(ErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <inheritdoc/>
    public override string ToString() => Message;
}

/// <summary>
/// Error codes for managed API exceptions.
/// </summary>
public enum ErrorCode
{
    // TODO: Revise these codes before release
    /// <summary>
    /// Internal Server Error.
    /// </summary>
    INTERNAL_SERVER_ERROR = 1,

    /// <summary>
    /// Unauthorized access.
    /// </summary>
    UNAUTHORIZED = 2,

    /// <summary>
    /// Local file not found.
    /// </summary>
    FILE_NOT_FOUND = 50,

    /// <summary>
    /// Remote resource not found.
    /// </summary>
    REMOTE_RESOURCE_NOT_FOUND = 51,

    /// <summary>
    /// Remote resource fetch failed.
    /// </summary>
    REMOTE_RESOURCE_FETCH_FAILED = 52,

    /// <summary>
    /// Local file already exists.
    /// </summary>
    FILE_ALREADY_EXISTS = 60,

    /// <summary>
    /// Guest user not found.
    /// </summary>
    GUEST_NOT_FOUND = 1001,

    /// <summary>
    /// Plugin not found.
    /// </summary>
    PLUGIN_NOT_FOUND = 1002,

    /// <summary>
    /// Wordlist not found.
    /// </summary>
    WORDLIST_NOT_FOUND = 1003,

    /// <summary>
    /// Proxy group not found.
    /// </summary>
    PROXY_GROUP_NOT_FOUND = 1004,

    /// <summary>
    /// Config not found.
    /// </summary>
    CONFIG_NOT_FOUND = 1005,

    /// <summary>
    /// The proxy group is being used in a job.
    /// </summary>
    PROXY_GROUP_IN_USE = 2001,
}
