namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Request to start a multi run job.
/// </summary>
public class MrjStartRequest
{
    Dictionary<string, string> CustomInputAnswers { get; set; } = new();
}
