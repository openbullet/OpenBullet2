namespace RuriLib.Functions.Imap;

/// <summary>
/// Controls how IMAP auto-connect discovers candidate servers.
/// </summary>
public enum ImapAutoConnectMode
{
    /// <summary>
    /// Try known servers first, then fall back to all discovery strategies.
    /// </summary>
    Full,
    /// <summary>
    /// Only try the servers already known in <c>imapdomains.dat</c>.
    /// </summary>
    KnownServersOnly
}
