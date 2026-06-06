namespace RuriLib.Functions.Pop3;

/// <summary>
/// Controls how POP3 auto-connect discovers candidate servers.
/// </summary>
public enum Pop3AutoConnectMode
{
    /// <summary>
    /// Try known servers first, then fall back to all discovery strategies.
    /// </summary>
    Full,
    /// <summary>
    /// Only try the servers already known in <c>pop3domains.dat</c>.
    /// </summary>
    KnownServersOnly
}
