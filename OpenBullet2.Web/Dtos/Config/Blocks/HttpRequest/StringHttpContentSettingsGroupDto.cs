using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents a string multipart setting.
/// </summary>
[PolyType("multipartString")]
[MapsFrom(typeof(StringHttpContentSettingsGroup))]
[MapsTo(typeof(StringHttpContentSettingsGroup), false)]
public class StringHttpContentSettingsGroupDto : HttpContentSettingsGroupDto
{
    /// <summary>
    /// The string data.
    /// </summary>
    public BlockSettingDto? Data { get; set; }
}
