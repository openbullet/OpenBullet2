namespace RuriLib.Models.Configs.Settings;

/// <summary>
/// Defines general execution behavior for a config.
/// </summary>
public class GeneralSettings
{
    /// <summary>
    /// The suggested bot count.
    /// </summary>
    public int SuggestedBots { get; set; } = 1;

    /// <summary>
    /// The maximum suggested CPM.
    /// </summary>
    public int MaximumCPM { get; set; }

    /// <summary>
    /// Whether empty captures should be saved.
    /// </summary>
    public bool SaveEmptyCaptures { get; set; }

    /// <summary>
    /// Whether the last captcha should be reported on retry.
    /// </summary>
    public bool ReportLastCaptchaOnRetry { get; set; }

    /// <summary>
    /// Statuses that should continue execution instead of stopping.
    /// </summary>
    public string[] ContinueStatuses { get; set; } =
    [
        "SUCCESS",
        "NONE"
    ];
}
