using OpenBullet2.Web.Dtos.Config.Settings;

namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains a config's settings.
/// </summary>
public class ConfigSettingsDto
{
    /// <summary>
    /// The general settings.
    /// </summary>
    public ConfigGeneralSettingsDto GeneralSettings { get; set; } = new();

    /// <summary>
    /// The proxy-related settings.
    /// </summary>
    public ConfigProxySettingsDto ProxySettings { get; set; } = new();

    /// <summary>
    /// The input settings.
    /// </summary>
    public ConfigInputSettingsDto InputSettings { get; set; } = new();

    /// <summary>
    /// The data-related settings.
    /// </summary>
    public ConfigDataSettingsDto DataSettings { get; set; } = new();

    /// <summary>
    /// The browser-related settings.
    /// </summary>
    public ConfigBrowserSettingsDto BrowserSettings { get; set; } = new();

    /// <summary>
    /// The script-related settings.
    /// </summary>
    public ConfigScriptSettingsDto ScriptSettings { get; set; } = new();
}
