using OpenBullet2.Web.Exceptions;

namespace OpenBullet2.Web.Models.Errors;

/// <summary>
/// A generic error from the API.
/// </summary>
public class ApiError
{
    /// <summary>
    /// The error code.
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// The error type.
    /// </summary>
    public string ErrorType { get; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Additional details, if any, such as the stack trace in the
    /// case of internal server errors.
    /// </summary>
    public string? Details { get; set; }

    /// <summary></summary>
    public ApiError(ErrorCode errorCode,
        string message, string? details = null)
    {
        ErrorCode = (int)errorCode;
        ErrorType = errorCode.ToString();
        Message = message;
        Details = details;
    }
}
