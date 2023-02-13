using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents a raw multipart setting.
/// </summary>
[PolyType("multipartRaw")]
[MapsFrom(typeof(RawHttpContentSettingsGroup))]
public class RawHttpContentSettingsGroupDto : HttpContentSettingsGroupDto
{
    /// <summary>
    /// The raw data.
    /// </summary>
    public BlockSettingDto? Data { get; set; }
}
