namespace OpenBullet2.Web.Dtos.Settings;

/// <summary>
/// Settings related to the appearance of the OpenBullet2 GUI.
/// </summary>
public class OBCustomizationSettingsDto
{
    /// <summary>
    /// The theme to use. Themes are included in separate files and identified
    /// by their name. Web UI only.
    /// </summary>
    public string Theme { get; set; } = "Default";

    /// <summary>
    /// The theme to use for the Monaco editor. Web UI only.
    /// </summary>
    public string MonacoTheme { get; set; } = "vs-dark";

    /// <summary>
    /// Whether to wrap words at viewport width.
    /// </summary>
    public bool WordWrap { get; set; } = false;

    /// <summary>
    /// Whether to play a sound when a hit is found.
    /// </summary>
    public bool PlaySoundOnHit { get; set; } = false;
}
