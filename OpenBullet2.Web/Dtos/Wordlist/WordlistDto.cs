using OpenBullet2.Web.Dtos.User;

namespace OpenBullet2.Web.Dtos.Wordlist;

/// <summary>
/// DTO that represents a wordlist.
/// </summary>
public class WordlistDto
{
    /// <summary>
    /// The id of the wordlist.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the wordlist.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The path to the actual file on disk.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// The purpose of the wordlist.
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// The total number of lines in the wordlist.
    /// </summary>
    public int LineCount { get; set; }

    /// <summary>
    /// The wordlist type.
    /// </summary>
    public string WordlistType { get; set; } = string.Empty;

    /// <summary>
    /// The owner of this wordlist. Null if owned by the admin user.
    /// </summary>
    public OwnerDto? Owner { get; set; }
}
