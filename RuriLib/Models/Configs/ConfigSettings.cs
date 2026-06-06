using RuriLib.Models.Configs.Settings;
using System.Text.Json.Serialization;

namespace RuriLib.Models.Configs;

/// <summary>
/// Groups all editable settings sections of a config.
/// </summary>
public class ConfigSettings
{
    /// <summary>
    /// General execution settings.
    /// </summary>
    public GeneralSettings GeneralSettings { get; set; } = new();

    /// <summary>
    /// Proxy-related settings.
    /// </summary>
    public ProxySettings ProxySettings { get; set; } = new();

    /// <summary>
    /// Custom input settings.
    /// </summary>
    public InputSettings InputSettings { get; set; } = new();

    /// <summary>
    /// Data slicing and resource settings.
    /// </summary>
    public DataSettings DataSettings { get; set; } = new();

    /// <summary>
    /// Browser automation settings.
    /// </summary>
    [JsonPropertyName("PuppeteerSettings")] // For backwards compatibility
    public BrowserSettings BrowserSettings { get; set; } = new();

    /// <summary>
    /// C# script compilation settings.
    /// </summary>
    public ScriptSettings ScriptSettings { get; set; } = new();
}
