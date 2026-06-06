using RuriLib.Functions.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuriLib.Providers.Emails;

/// <summary>
/// Stores and retrieves known mail server hosts for email domains.
/// </summary>
public interface IEmailDomainRepository
{
    /// <summary>
    /// Gets known IMAP servers for a domain.
    /// </summary>
    /// <param name="domain">The email domain.</param>
    /// <returns>The known IMAP host entries.</returns>
    Task<IEnumerable<HostEntry>> GetImapServers(string domain);

    /// <summary>
    /// Gets known POP3 servers for a domain.
    /// </summary>
    /// <param name="domain">The email domain.</param>
    /// <returns>The known POP3 host entries.</returns>
    Task<IEnumerable<HostEntry>> GetPop3Servers(string domain);

    /// <summary>
    /// Gets known SMTP servers for a domain.
    /// </summary>
    /// <param name="domain">The email domain.</param>
    /// <returns>The known SMTP host entries.</returns>
    Task<IEnumerable<HostEntry>> GetSmtpServers(string domain);

    /// <summary>
    /// Adds an IMAP server for a domain if it is not already known.
    /// </summary>
    /// <param name="domain">The email domain.</param>
    /// <param name="server">The server to add.</param>
    Task TryAddImapServer(string domain, HostEntry server);

    /// <summary>
    /// Adds a POP3 server for a domain if it is not already known.
    /// </summary>
    /// <param name="domain">The email domain.</param>
    /// <param name="server">The server to add.</param>
    Task TryAddPop3Server(string domain, HostEntry server);

    /// <summary>
    /// Adds an SMTP server for a domain if it is not already known.
    /// </summary>
    /// <param name="domain">The email domain.</param>
    /// <param name="server">The server to add.</param>
    Task TryAddSmtpServer(string domain, HostEntry server);
}
