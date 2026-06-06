using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.HttpRequest;

/// <summary>
/// Parameters for a raw binary HTTP request.
/// </summary>
public class RawRequestParams : RequestParams
{
    /// <summary>
    /// Gets or sets the raw request content.
    /// </summary>
    public BlockSetting Content { get; set; } = BlockSettingFactory.CreateByteArraySetting("content");
    /// <summary>
    /// Gets or sets the request content type.
    /// </summary>
    public BlockSetting ContentType { get; set; } = BlockSettingFactory.CreateStringSetting("contentType", "application/octet-stream");
}
