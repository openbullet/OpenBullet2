using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

/// <summary>
/// Settings group for string multipart content.
/// </summary>
public class StringHttpContentSettingsGroup : HttpContentSettingsGroup
{
    /// <summary>
    /// Gets or sets the string payload.
    /// </summary>
    public BlockSetting Data { get; set; } = BlockSettingFactory.CreateStringSetting("data");

    /// <summary>
    /// Initializes a new <see cref="StringHttpContentSettingsGroup"/>.
    /// </summary>
    public StringHttpContentSettingsGroup()
    {
        if (ContentType.FixedSetting is StringSetting contentTypeSetting)
        {
            contentTypeSetting.Value = "text/plain";
        }
    }
}
