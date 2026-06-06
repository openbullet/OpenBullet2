namespace RuriLib.Functions.Imap;

/// <summary>
/// Identifies the IMAP message field to search.
/// </summary>
public enum SearchField
{
    /// <summary>
    /// Search by sender.
    /// </summary>
    From,
    /// <summary>
    /// Search by recipient.
    /// </summary>
    To,
    /// <summary>
    /// Search by subject.
    /// </summary>
    Subject,
    /// <summary>
    /// Search by message body.
    /// </summary>
    Body
}
