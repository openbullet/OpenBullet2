using OpenBullet2.Web.Exceptions;

namespace OpenBullet2.Web.Models.Errors;

public class ApiError
{
    public int ErrorCode { get; set; }
    public string ErrorType { get; }
    public string Message { get; set; }
    public string? Details { get; set; }

    public ApiError(ErrorCode errorCode,
        string message, string? details = null)
    {
        ErrorCode = (int)errorCode;
        ErrorType = errorCode.ToString();
        Message = message;
        Details = details;
    }
}
