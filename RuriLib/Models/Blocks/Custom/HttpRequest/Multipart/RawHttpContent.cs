namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

/// <summary>
/// Runtime multipart content containing raw binary data.
/// </summary>
public class RawHttpContent(string name, byte[] data, string contentType) : MyHttpContent(name, contentType)
{
    /// <summary>
    /// Gets or sets the binary payload.
    /// </summary>
    public byte[] Data { get; set; } = data;
}
