namespace OpenBullet2.Web.Exceptions;

public class ApiException : Exception
{
    public ErrorCode ErrorCode { get; set; }

    public ApiException(ErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

public enum ErrorCode
{
    INTERNAL_SERVER_ERROR = 1,
    UNAUTHORIZED = 2,
    GUEST_NOT_FOUND = 1001
}
