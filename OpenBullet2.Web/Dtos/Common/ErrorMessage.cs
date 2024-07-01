namespace OpenBullet2.Web.Dtos.Common;

/// <summary>
/// A new error was raised.
/// </summary>
public class ErrorMessage
{
    /// <summary></summary>
    public ErrorMessage(string message)
    {
        Message = message;
    }

    /// <summary></summary>
    public ErrorMessage()
    {
    }

    /// <summary>
    /// The error type.
    /// </summary>
    public string Type { get; set; } = nameof(Exception);

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The full stack trace of the exception.
    /// </summary>
    public string StackTrace { get; set; } = string.Empty;
}
