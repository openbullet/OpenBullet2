namespace OpenBullet2.Web.Models.Errors;

public class ApiError
{
    public int ErrorCode { get; set; }
    public string Message { get; set; }
    public string? Details { get; set; }

    public ApiError(int errorCode, string message, string? details = null)
    {
        ErrorCode = errorCode;
        Message = message;
        Details = details;
    }
}
