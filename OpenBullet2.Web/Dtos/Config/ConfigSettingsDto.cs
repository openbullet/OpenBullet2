using OpenBullet2.Web.Dtos.Config.Settings;
using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains a config's settings.
/// </summary>
public class ConfigSettingsDto
{
    /// <summary>
    /// The general settings.
    /// </summary>
    [Required]
    public ConfigGeneralSettingsDto GeneralSettings { get; set; } = new();

    /// <summary>
    /// The proxy-related settings.
    /// </summary>
    [Required]
    public ConfigProxySettingsDto ProxySettings { get; set; } = new();

    /// <summary>
    /// The input settings.
    /// </summary>
    [Required]
    public ConfigInputSettingsDto InputSettings { get; set; } = new();

    /// <summary>
    /// The data-related settings.
    /// </summary>
    [Required]
    public ConfigDataSettingsDto DataSettings { get; set; } = new();

    /// <summary>
    /// The browser-related settings.
    /// </summary>
    [Required]
    public ConfigBrowserSettingsDto BrowserSettings { get; set; } = new();

    /// <summary>
    /// The script-related settings.
    /// </summary>
    [Required]
    public ConfigScriptSettingsDto ScriptSettings { get; set; } = new();
}
