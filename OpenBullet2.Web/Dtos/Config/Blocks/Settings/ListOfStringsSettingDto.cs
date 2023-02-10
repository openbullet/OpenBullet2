namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents a fixed list of strings setting.
/// </summary>
public class ListOfStringsSettingDto : SettingDto
{
    /// <summary>
    /// The list of strings value.
    /// </summary>
    public List<string> Value { get; set; } = new();
}
