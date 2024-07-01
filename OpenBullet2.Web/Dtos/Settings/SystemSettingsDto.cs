namespace OpenBullet2.Web.Dtos.Settings;

/// <summary>
/// System settings, cannot be changed after the application has started.
/// </summary>
public class SystemSettingsDto
{
    /// <summary>
    /// The maximum number of bots that can be run by a job at the same time.
    /// </summary>
    public int BotLimit { get; set; } = 200;
}
