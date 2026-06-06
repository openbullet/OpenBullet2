using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using System.Collections.Generic;

namespace RuriLib.Functions.Http.Options;

/// <summary>
/// Options for a multipart HTTP request body.
/// </summary>
public class MultipartHttpRequestOptions : HttpRequestOptions
{
    /// <summary>
    /// Gets or sets the multipart boundary.
    /// </summary>
    public string Boundary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the multipart contents.
    /// </summary>
    public List<MyHttpContent> Contents { get; set; } = new();
}
