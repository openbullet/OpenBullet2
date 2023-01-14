namespace OpenBullet2.Web.Exceptions;

public class ApiException : Exception
{
    public ErrorCode ErrorCode { get; set; }

    public ApiException(ErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public override string ToString() => Message;
}

public enum ErrorCode
{
    // TODO: Revise these codes before release
    INTERNAL_SERVER_ERROR = 1,
    UNAUTHORIZED = 2,
    REMOTE_RESOURCE_NOT_FOUND = 50,
    GUEST_NOT_FOUND = 1001,
    PLUGIN_NOT_FOUND = 1002
}
