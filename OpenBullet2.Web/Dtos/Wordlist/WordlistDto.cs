namespace OpenBullet2.Web.Dtos.Wordlist;

/// <summary>
/// DTO that represents a wordlist.
/// </summary>
public class WordlistDto
{
    /// <summary>
    /// The name of the wordlist.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// The path to the actual file on disk.
    /// </summary>
    public string FilePath { get; set; } = default!;

    /// <summary>
    /// The purpose of the wordlist.
    /// </summary>

    public string Purpose { get; set; } = default!;

    /// <summary>
    /// The total number of lines in the wordlist.
    /// </summary>
    public int LineCount { get; set; }

    /// <summary>
    /// The wordlist type.
    /// </summary>
    public string WordlistType { get; set; } = default!;

    /// <summary>
    /// The username of the owner of this wordlist.
    /// </summary>
    public string OwnerUsername { get; set; } = default!;
}
