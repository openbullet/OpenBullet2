namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains a config's input settings.
/// </summary>
public class ConfigDataSettingsDto
{
    /// <summary>
    /// The types of wordlists that can be used with this config.
    /// </summary>
    public string[] AllowedWordlistTypes { get; set; } = { "Default" };

    /// <summary>
    /// Whether to apply URL encoding to the data slices.
    /// </summary>
    public bool UrlEncodeDataAfterSlicing { get; set; } = false;

    /// <summary>
    /// The rules that will be used to check if the data is valid or not.
    /// </summary>
    public DataRulesDto DataRules { get; set; } = new();

    /// <summary>
    /// The configured resources.
    /// </summary>
    public ResourcesDto Resources { get; set; } = new();
}
