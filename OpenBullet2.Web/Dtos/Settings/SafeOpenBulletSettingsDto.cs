using OpenBullet2.Core.Models.Settings;

namespace OpenBullet2.Web.Dtos.Settings;

/// <summary>
/// Safe settings for the OpenBullet 2 application.
/// </summary>
public class SafeOpenBulletSettingsDto
{
    /// <summary>
    /// Safe general settings.
    /// </summary>
    public SafeOBGeneralSettingsDto GeneralSettings { get; set; } = new();
    
    /// <summary>
    /// Safe customization settings.
    /// </summary>
    public SafeOBCustomizationSettingsDto CustomizationSettings { get; set; } = new();
}

/// <summary>
/// Safe general settings of OpenBullet 2.
/// </summary>
public class SafeOBGeneralSettingsDto
{
    /// <summary>
    /// Which page to navigate to on config load.
    /// </summary>
    public ConfigSection ConfigSectionOnLoad { get; set; } = ConfigSection.Stacker;
    
    /// <summary>
    /// The refresh interval for periodically displaying all jobs' progress and information
    /// in the job manager page (in milliseconds).
    /// </summary>
    public int JobManagerUpdateInterval { get; set; } = 1000;
    
    /// <summary>
    /// The default display mode for job information.
    /// </summary>
    public JobDisplayMode DefaultJobDisplayMode { get; set; } = JobDisplayMode.Standard;
}

/// <summary>
/// Safe customization settings of OpenBullet 2.
/// </summary>
public class SafeOBCustomizationSettingsDto
{
    /// <summary>
    /// Whether to play a sound when a hit is found.
    /// </summary>
    public bool PlaySoundOnHit { get; set; } = false;
    
    /// <summary>
    /// Whether to wrap words at viewport width in the code editor.
    /// </summary>
    public bool WordWrap { get; set; } = false;
}
