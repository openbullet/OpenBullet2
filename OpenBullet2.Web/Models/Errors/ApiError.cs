namespace OpenBullet2.Web.Models.Errors;

/// <summary>
/// A generic error from the API.
/// </summary>
public class ApiError
{
    /// <summary></summary>
    public ApiError(string errorCode,
        string message, string? details = null)
    {
        ErrorCode = errorCode;
        Message = message;
        Details = details;
    }

    /// <summary>
    /// The error code.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Additional details, if any, such as the stack trace in the
    /// case of internal server errors.
    /// </summary>
    public string? Details { get; set; }
}
