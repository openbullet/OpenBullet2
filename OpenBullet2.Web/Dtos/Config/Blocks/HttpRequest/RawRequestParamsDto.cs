using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.HttpRequest;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents raw request params.
/// </summary>
[PolyType("rawRequestParams")]
[MapsFrom(typeof(RawRequestParams))]
[MapsTo(typeof(RawRequestParams), false)]
public class RawRequestParamsDto : RequestParamsDto
{
    /// <summary>
    /// The request content.
    /// </summary>
    public BlockSettingDto? Content { get; set; }

    /// <summary>
    /// The content type.
    /// </summary>
    public BlockSettingDto? ContentType { get; set; }
}
