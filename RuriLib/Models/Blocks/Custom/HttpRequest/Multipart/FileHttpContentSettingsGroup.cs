using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

/// <summary>
/// Settings group for file multipart content.
/// </summary>
public class FileHttpContentSettingsGroup : HttpContentSettingsGroup
{
    /// <summary>
    /// Gets or sets the file name or path.
    /// </summary>
    public BlockSetting FileName { get; set; } = BlockSettingFactory.CreateStringSetting("fileName");

    /// <summary>
    /// Initializes a new <see cref="FileHttpContentSettingsGroup"/>.
    /// </summary>
    public FileHttpContentSettingsGroup()
    {
        if (ContentType.FixedSetting is StringSetting contentTypeSetting)
        {
            contentTypeSetting.Value = "application/octet-stream";
        }
    }
}
