using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.HttpRequest;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents standard request params.
/// </summary>
[PolyType("standardRequestParams")]
[MapsFrom(typeof(StandardRequestParams))]
[MapsTo(typeof(StandardRequestParams), false)]
public class StandardRequestParamsDto : RequestParamsDto
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
