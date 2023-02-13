using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Config.Blocks.Settings;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents a file multipart setting.
/// </summary>
[PolyType("multipartFile")]
[MapsFrom(typeof(FileHttpContentSettingsGroup))]
public class FileHttpContentSettingsGroupDto : HttpContentSettingsGroupDto
{
    /// <summary></summary>
    public FileHttpContentSettingsGroupDto()
    {
        Type = HttpContentSettingsGroupType.File;
    }

    /// <summary>
    /// The file name.
    /// </summary>
    public BlockSettingDto? FileName { get; set; }
}
