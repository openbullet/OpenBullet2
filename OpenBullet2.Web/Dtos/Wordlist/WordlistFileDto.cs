namespace OpenBullet2.Web.Dtos.Wordlist;

/// <summary>
/// DTO that represents the file referenced by a wordlist.
/// </summary>
public class WordlistFileDto
{
    /// <summary>
    /// The path of the file on disk.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}
