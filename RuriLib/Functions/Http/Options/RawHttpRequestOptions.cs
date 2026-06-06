using System;

namespace RuriLib.Functions.Http.Options;

/// <summary>
/// Options for a raw byte-array HTTP request body.
/// </summary>
public class RawHttpRequestOptions : HttpRequestOptions
{
    /// <summary>
    /// Gets or sets the request body content.
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the request content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
}
