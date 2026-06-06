using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

/// <summary>
/// Base settings group for a multipart content entry.
/// </summary>
public class HttpContentSettingsGroup
{
    /// <summary>
    /// Gets or sets the content name.
    /// </summary>
    public BlockSetting Name { get; set; } = BlockSettingFactory.CreateStringSetting("name");
    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public BlockSetting ContentType { get; set; } = BlockSettingFactory.CreateStringSetting("contentType");
}
