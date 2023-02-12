using RuriLib.Functions.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RuriLib.Providers.Emails
{
    public class FileEmailDomainRepository : IEmailDomainRepository
    {
        private const string imapFile = "UserData/imapdomains.dat";
        private const string pop3File = "UserData/pop3domains.dat";
        private const string smtpFile = "UserData/smtpdomains.dat";

        private readonly ConcurrentDictionary<string, List<HostEntry>> imapHosts = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, List<HostEntry>> pop3Hosts = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, List<HostEntry>> smtpHosts = new(StringComparer.OrdinalIgnoreCase);

        public FileEmailDomainRepository()
        {
            FillDictionary(imapFile, imapHosts);
            FillDictionary(pop3File, pop3Hosts);
            FillDictionary(smtpFile, smtpHosts);
        }

        public Task TryAddImapServer(string domain, HostEntry server)
            => TryAddServer(imapFile, imapHosts, domain, server);

        public Task TryAddPop3Server(string domain, HostEntry server)
            => TryAddServer(pop3File, pop3Hosts, domain, server);

        public Task TryAddSmtpServer(string domain, HostEntry server)
            => TryAddServer(smtpFile, smtpHosts, domain, server);

        public Task<IEnumerable<HostEntry>> GetImapServers(string domain)
            => Task.FromResult((imapHosts.ContainsKey(domain) ? imapHosts[domain] : new List<HostEntry>()) as IEnumerable<HostEntry>);

        public Task<IEnumerable<HostEntry>> GetPop3Servers(string domain)
            => Task.FromResult((pop3Hosts.ContainsKey(domain) ? pop3Hosts[domain] : new List<HostEntry>()) as IEnumerable<HostEntry>);

        public Task<IEnumerable<HostEntry>> GetSmtpServers(string domain)
            => Task.FromResult((smtpHosts.ContainsKey(domain) ? smtpHosts[domain] : new List<HostEntry>()) as IEnumerable<HostEntry>);

        private static void FillDictionary(string file, ConcurrentDictionary<string, List<HostEntry>> hosts)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, string.Empty);
            }

            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {
                try
                {
                    var split = line.Split(':');
                    var entry = new HostEntry(split[1], int.Parse(split[2]));

                    // If we already added an entry for this domain, add it to the list
                    if (hosts.ContainsKey(split[0]))
                    {
                        hosts[split[0]].Add(entry);
                    }
                    else
                    {
                        hosts[split[0]] = new List<HostEntry> { entry };
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static Task TryAddServer(string file, ConcurrentDictionary<string, List<HostEntry>> hosts, string domain, HostEntry server)
        {
            if (!hosts.ContainsKey(domain))
            {
                hosts[domain] = new List<HostEntry> { server };

                try
                {
                    File.AppendAllText(file, $"{domain}:{server.Host}:{server.Port}{Environment.NewLine}");
                }
                catch
                {
                    // ignored
                }
            }

            return Task.CompletedTask;
        }
    }
}
