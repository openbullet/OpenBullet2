using OpenBullet2.Web.Dtos.Config.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents a raw multipart setting.
/// </summary>
public class RawHttpContentSettingsGroupDto : HttpContentSettingsGroupDto
{
    /// <summary></summary>
    public RawHttpContentSettingsGroupDto()
    {
        Type = HttpContentSettingsGroupType.Raw;
    }

    /// <summary>
    /// The raw data.
    /// </summary>
    public BlockSettingDto? Data { get; set; }
}
