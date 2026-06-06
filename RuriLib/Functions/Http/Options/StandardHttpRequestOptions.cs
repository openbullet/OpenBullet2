namespace RuriLib.Functions.Http.Options;

/// <summary>
/// Options for a standard string-based HTTP request body.
/// </summary>
public class StandardHttpRequestOptions : HttpRequestOptions
{
    /// <summary>
    /// Gets or sets the request body content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the content should be URL encoded.
    /// </summary>
    public bool UrlEncodeContent { get; set; }
}
