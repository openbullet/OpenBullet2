namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// An error happened in a task.
/// </summary>
public class MRJTaskErrorMessage
{
    /// <summary>
    /// The data line that was being processed.
    /// </summary>
    public string DataLine { get; set; } = string.Empty;

    /// <summary>
    /// The proxy, if any.
    /// </summary>
    public MRJProxy? Proxy { get; set; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
