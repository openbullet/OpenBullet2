namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;

/// <summary>
/// Runtime multipart content pointing to a file payload.
/// </summary>
public class FileHttpContent(string name, string fileName, string contentType) : MyHttpContent(name, contentType)
{
    /// <summary>
    /// Gets or sets the file name or path.
    /// </summary>
    public string FileName { get; set; } = fileName;
}
