using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

/// <summary>
/// Settings group for raw binary multipart content.
/// </summary>
public class RawHttpContentSettingsGroup : HttpContentSettingsGroup
{
    /// <summary>
    /// Gets or sets the binary payload.
    /// </summary>
    public BlockSetting Data { get; set; } = BlockSettingFactory.CreateByteArraySetting("data");

    /// <summary>
    /// Initializes a new <see cref="RawHttpContentSettingsGroup"/>.
    /// </summary>
    public RawHttpContentSettingsGroup()
    {
        if (ContentType.FixedSetting is StringSetting contentTypeSetting)
        {
            contentTypeSetting.Value = "application/octet-stream";
        }
    }
}
