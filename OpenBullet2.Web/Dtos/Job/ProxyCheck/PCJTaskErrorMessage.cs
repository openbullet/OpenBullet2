namespace OpenBullet2.Web.Dtos.Job.ProxyCheck;

/// <summary>
/// An error happened in a task.
/// </summary>
public class PcjTaskErrorMessage
{
    /// <summary>
    /// The host of the proxy.
    /// </summary>
    public string ProxyHost { get; set; } = string.Empty;

    /// <summary>
    /// The port of the proxy.
    /// </summary>
    public int ProxyPort { get; set; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
