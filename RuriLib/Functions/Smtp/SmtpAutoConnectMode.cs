namespace RuriLib.Functions.Smtp;

/// <summary>
/// Controls how SMTP auto-connect discovers candidate servers.
/// </summary>
public enum SmtpAutoConnectMode
{
    /// <summary>
    /// Try known servers first, then fall back to all discovery strategies.
    /// </summary>
    Full,
    /// <summary>
    /// Only try the servers already known in <c>smtpdomains.dat</c>.
    /// </summary>
    KnownServersOnly
}
