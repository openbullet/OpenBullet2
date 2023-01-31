using OpenBullet2.Core.Models.Settings;

namespace OpenBullet2.Web.Dtos.Settings;

/// <summary>
/// General settings of OpenBullet 2.
/// </summary>
public class OBGeneralSettingsDto
{
    // This is the same as GeneralSettings but the name was clashing
    // with the one in RuriLib and the swagger gen was mad so I had
    // to create this DTO for this purpose.

    /// <summary>
    /// Which page to navigate to on config load.
    /// </summary>
    public ConfigSection ConfigSectionOnLoad { get; set; } = ConfigSection.Stacker;

    /// <summary>
    /// Whether to automatically set the recommended amount of bots specified by a config
    /// when selecting a config in a job.
    /// </summary>
    public bool AutoSetRecommendedBots { get; set; } = true;

    /// <summary>
    /// Whether to output a warning upon quitting or loading a new config when
    /// the previous one was edited but not saved.
    /// </summary>
    public bool WarnConfigNotSaved { get; set; } = true;

    /// <summary>
    /// The default author to use when creating new configs.
    /// </summary>
    public string DefaultAuthor { get; set; } = "Anonymous";

    /// <summary>
    /// Whether to display the job log in the interface.
    /// </summary>
    public bool EnableJobLogging { get; set; } = false;

    /// <summary>
    /// The maximum amount of log entries that are saved in memory for each job.
    /// </summary>
    public int LogBufferSize { get; set; } = 30;

    /// <summary>
    /// Whether to ignore the wordlist name when removing duplicate hits (so that similar hits
    /// obtained using different wordlists are treated as duplicate).
    /// </summary>
    public bool IgnoreWordlistNameOnHitsDedupe { get; set; } = false;

    /// <summary>
    /// The available targets that can be used to check proxies.
    /// </summary>
    public List<ProxyCheckTarget> ProxyCheckTargets { get; set; } = new();

    /// <summary>
    /// The default display mode for job information.
    /// </summary>
    public JobDisplayMode DefaultJobDisplayMode { get; set; } = JobDisplayMode.Standard;

    /// <summary>
    /// The refresh interval for periodically displaying a job's progress and information
    /// (in milliseconds).
    /// </summary>
    public int JobUpdateInterval { get; set; } = 1000;

    /// <summary>
    /// The refresh interval for periodically displaying all jobs' progress and information
    /// in the job manager page (in milliseconds).
    /// </summary>
    public int JobManagerUpdateInterval { get; set; } = 1000;

    /// <summary>
    /// Whether to group captured variables together in the variables log of the debugger.
    /// </summary>
    public bool GroupCapturesInDebugger { get; set; } = false;

    /// <summary>
    /// The localization culture for the UI.
    /// </summary>
    public string Culture { get; set; } = "en";

    /// <summary>
    /// Custom user-defined snippets for editor autocompletion.
    /// </summary>
    public List<CustomSnippet> CustomSnippets { get; set; } = new();
}
