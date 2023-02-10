namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents a fixed dictionary of strings setting.
/// </summary>
public class DictionaryOfStringsSettingDto : SettingDto
{
    /// <summary>
    /// The dictionary of strings value.
    /// </summary>
    public Dictionary<string, string> Value { get; set; } = new();
}
