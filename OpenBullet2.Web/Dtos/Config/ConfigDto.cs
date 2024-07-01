using RuriLib.Models.Configs;

namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains a config's data.
/// </summary>
public class ConfigDto
{
    /// <summary>
    /// The unique id of the config.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Whether the config was downloaded from a remote source and should not
    /// be edited to avoid synchronization issues.
    /// </summary>
    public bool IsRemote { get; set; }

    /// <summary>
    /// The current config mode.
    /// </summary>
    public ConfigMode Mode { get; set; } = ConfigMode.LoliCode;

    /// <summary>
    /// The metadata of the config.
    /// </summary>
    public ConfigMetadataDto Metadata { get; set; } = default!;

    /// <summary>
    /// The config's settings.
    /// </summary>
    public ConfigSettingsDto Settings { get; set; } = default!;

    /// <summary>
    /// The markdown body of the readme.
    /// </summary>
    public string Readme { get; set; } = string.Empty;

    /// <summary>
    /// The LoliCode script.
    /// </summary>
    public string LoliCodeScript { get; set; } = string.Empty;

    /// <summary>
    /// The LoliCode script that gets executed once, before anything else.
    /// </summary>
    public string StartupLoliCodeScript { get; set; } = string.Empty;

    /// <summary>
    /// The LoliScript code of legacy configs.
    /// </summary>
    public string LoliScript { get; set; } = string.Empty;

    /// <summary>
    /// The C# script that gets executed once, before anything else.
    /// </summary>
    public string StartupCSharpScript { get; set; } = string.Empty;

    /// <summary>
    /// The C# script for configs that were converted to C#.
    /// </summary>
    public string CSharpScript { get; set; } = string.Empty;
}
