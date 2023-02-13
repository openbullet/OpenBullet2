using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Config.Blocks.Settings;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents a raw multipart setting.
/// </summary>
[PolyType("multipartRaw")]
[MapsFrom(typeof(RawHttpContentSettingsGroup))]
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
