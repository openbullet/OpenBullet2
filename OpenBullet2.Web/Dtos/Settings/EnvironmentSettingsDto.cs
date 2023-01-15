using RuriLib.Models.Environment;

namespace OpenBullet2.Web.Dtos.Settings;

/// <summary>
/// DTO that represents the environment settings.
/// </summary>
public class EnvironmentSettingsDto
{
    /// <summary>
    /// The available wordlist types.
    /// </summary>
    public List<WordlistType> WordlistTypes { get; set; } = new();

    /// <summary>
    /// The available custom statuses.
    /// </summary>
    public List<CustomStatus> CustomStatuses { get; set; } = new();

    /// <summary>
    /// The available export formats.
    /// </summary>
    public List<ExportFormat> ExportFormats { get; set; } = new();
}
