using OpenBullet2.Web.Dtos.Config.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents a string multipart setting.
/// </summary>
public class StringHttpContentSettingsGroupDto : HttpContentSettingsGroupDto
{
    /// <summary></summary>
    public StringHttpContentSettingsGroupDto()
    {
        Type = HttpContentSettingsGroupType.String;
    }

    /// <summary>
    /// The string data.
    /// </summary>
    public BlockSettingDto? Data { get; set; }
}
