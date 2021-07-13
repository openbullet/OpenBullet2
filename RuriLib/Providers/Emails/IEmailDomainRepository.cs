using RuriLib.Functions.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuriLib.Providers.Emails
{
    public interface IEmailDomainRepository
    {
        Task<IEnumerable<HostEntry>> GetImapServers(string domain);
        Task<IEnumerable<HostEntry>> GetPop3Servers(string domain);
        Task<IEnumerable<HostEntry>> GetSmtpServers(string domain);

        Task TryAddImapServer(string domain, HostEntry server);
        Task TryAddPop3Server(string domain, HostEntry server);
        Task TryAddSmtpServer(string domain, HostEntry server);
    }
}
