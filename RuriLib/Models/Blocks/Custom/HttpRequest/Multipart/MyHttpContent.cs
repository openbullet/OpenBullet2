namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

/// <summary>
/// Base runtime multipart content payload.
/// </summary>
public abstract class MyHttpContent(string name, string contentType)
{
    /// <summary>
    /// Gets or sets the content name.
    /// </summary>
    public string Name { get; set; } = name;
    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; } = contentType;
}
