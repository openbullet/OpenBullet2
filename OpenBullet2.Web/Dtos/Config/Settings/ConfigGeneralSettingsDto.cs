namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains a config's general settings.
/// </summary>
public class ConfigGeneralSettingsDto
{
    /// <summary>
    /// The number of bots that is suggested to be used when running this config.
    /// </summary>
    public int SuggestedBots { get; set; } = 1;

    /// <summary>
    /// The limit to the CPM that a config can reach while running,
    /// where 0 is unlimited.
    /// </summary>
    public int MaximumCPM { get; set; } = 0;

    /// <summary>
    /// Whether to save variables that have an empty string value
    /// in the captured data.
    /// </summary>
    public bool SaveEmptyCaptures { get; set; } = false;

    /// <summary>
    /// Whether to automatically report the last captcha as incorrect
    /// to the configured captcha service (if any) when a bot ends with
    /// the RETRY status.
    /// </summary>
    public bool ReportLastCaptchaOnRetry { get; set; } = false;

    /// <summary>
    /// The statuses for which the bot should continue executing the
    /// config's script. When the <code>data.STATUS</code> variable
    /// assumes a value that is different from the ones provided,
    /// the bot will stop executing the config.
    /// </summary>
    public string[] ContinueStatuses { get; set; } = { "SUCCESS", "NONE" };
}
