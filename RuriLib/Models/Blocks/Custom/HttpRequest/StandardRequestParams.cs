using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest;

/// <summary>
/// Parameters for a standard content-based HTTP request.
/// </summary>
public class StandardRequestParams : RequestParams
{
    /// <summary>
    /// Gets or sets the request body content.
    /// </summary>
    public BlockSetting Content { get; set; } = BlockSettingFactory.CreateStringSetting("content", string.Empty, SettingInputMode.Interpolated);
    /// <summary>
    /// Gets or sets the request content type.
    /// </summary>
    public BlockSetting ContentType { get; set; } = BlockSettingFactory.CreateStringSetting("contentType", "application/x-www-form-urlencoded");
}
