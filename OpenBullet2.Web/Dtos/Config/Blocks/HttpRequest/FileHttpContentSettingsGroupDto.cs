using OpenBullet2.Web.Dtos.Config.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents a file multipart setting.
/// </summary>
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
