namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

/// <summary>
/// Runtime multipart content containing text data.
/// </summary>
public class StringHttpContent(string name, string data, string contentType) : MyHttpContent(name, contentType)
{
    /// <summary>
    /// Gets or sets the string payload.
    /// </summary>
    public string Data { get; set; } = data;
}
