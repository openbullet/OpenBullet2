using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Blocks.Settings;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Custom.HttpRequest;

/// <summary>
/// Parameters for a multipart HTTP request.
/// </summary>
public class MultipartRequestParams : RequestParams
{
    /// <summary>
    /// Gets or sets the multipart content entries.
    /// </summary>
    public List<HttpContentSettingsGroup> Contents { get; set; } = [];
    /// <summary>
    /// Gets or sets the multipart boundary.
    /// </summary>
    public BlockSetting Boundary { get; set; } = BlockSettingFactory.CreateStringSetting("boundary");
}
