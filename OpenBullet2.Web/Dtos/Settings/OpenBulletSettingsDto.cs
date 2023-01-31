using OpenBullet2.Core.Models.Settings;

namespace OpenBullet2.Web.Dtos.Settings;


/// <summary>
/// Settings for the OpenBullet 2 application.
/// </summary>
public class OpenBulletSettingsDto
{
    /// <summary>
    /// General settings.
    /// </summary>
    public GeneralSettings GeneralSettings { get; set; } = new();

    /// <summary>
    /// Settings related to remote repositories.
    /// </summary>
    public RemoteSettings RemoteSettings { get; set; } = new();

    /// <summary>
    /// Settings related to security.
    /// </summary>
    public SecuritySettingsDto SecuritySettings { get; set; } = new();

    /// <summary>
    /// Settings related to the appearance of the UI.
    /// </summary>
    public CustomizationSettingsDto CustomizationSettings { get; set; } = new();
}
