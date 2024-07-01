namespace OpenBullet2.Web.Dtos.Wordlist;

/// <summary>
/// DTO that contains the preview of the contents of a wordlist.
/// </summary>
public class WordlistPreviewDto
{
    /// <summary>
    /// The first few lines of a wordlist.
    /// </summary>
    public string[] FirstLines { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The size of the file, in bytes.
    /// </summary>
    public long SizeInBytes { get; set; }
}
