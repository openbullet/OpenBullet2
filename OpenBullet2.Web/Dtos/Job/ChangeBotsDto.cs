namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Information needed to change the number of bots in a job.
/// </summary>
public class ChangeBotsDto
{
    /// <summary>
    /// The id of the job.
    /// </summary>
    public int JobId { get; set; }
    
    /// <summary>
    /// The desired number of bots.
    /// </summary>
    public int Bots { get; set; }
}