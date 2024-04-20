namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Details about a bot in a multi run job.
/// </summary>
public class BotDetailsDto
{
    /// <summary>
    /// The id of the bot.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The data that the bot is processing.
    /// </summary>
    public string Data { get; set; } = string.Empty;
    
    /// <summary>
    /// The proxy that the bot is using, if any.
    /// </summary>
    public string? Proxy { get; set; }
    
    /// <summary>
    /// Information about what the bot is doing.
    /// </summary>
    public string Info { get; set; } = string.Empty;
}
