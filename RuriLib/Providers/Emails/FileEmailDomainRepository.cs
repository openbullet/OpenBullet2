using RuriLib.Functions.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RuriLib.Providers.Emails;

/// <summary>
/// File-backed implementation of <see cref="IEmailDomainRepository"/>.
/// </summary>
public class FileEmailDomainRepository : IEmailDomainRepository
{
    private const string _imapFile = "UserData/imapdomains.dat";
    private const string _pop3File = "UserData/pop3domains.dat";
    private const string _smtpFile = "UserData/smtpdomains.dat";

    private readonly ConcurrentDictionary<string, List<HostEntry>> _imapHosts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<HostEntry>> _pop3Hosts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<HostEntry>> _smtpHosts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a file-backed repository and loads the known server lists.
    /// </summary>
    public FileEmailDomainRepository()
    {
        FillDictionary(_imapFile, _imapHosts);
        FillDictionary(_pop3File, _pop3Hosts);
        FillDictionary(_smtpFile, _smtpHosts);
    }

    /// <inheritdoc />
    public Task TryAddImapServer(string domain, HostEntry server)
        => TryAddServer(_imapFile, _imapHosts, domain, server);

    /// <inheritdoc />
    public Task TryAddPop3Server(string domain, HostEntry server)
        => TryAddServer(_pop3File, _pop3Hosts, domain, server);

    /// <inheritdoc />
    public Task TryAddSmtpServer(string domain, HostEntry server)
        => TryAddServer(_smtpFile, _smtpHosts, domain, server);

    /// <inheritdoc />
    public Task<IEnumerable<HostEntry>> GetImapServers(string domain)
        => Task.FromResult(_imapHosts.TryGetValue(domain, out var hosts) ? hosts as IEnumerable<HostEntry> : []);

    /// <inheritdoc />
    public Task<IEnumerable<HostEntry>> GetPop3Servers(string domain)
        => Task.FromResult(_pop3Hosts.TryGetValue(domain, out var hosts) ? hosts as IEnumerable<HostEntry> : []);

    /// <inheritdoc />
    public Task<IEnumerable<HostEntry>> GetSmtpServers(string domain)
        => Task.FromResult(_smtpHosts.TryGetValue(domain, out var hosts) ? hosts as IEnumerable<HostEntry> : []);

    private static void FillDictionary(string file, ConcurrentDictionary<string, List<HostEntry>> hosts)
    {
        EnsureFileExists(file);

        var lines = File.ReadAllLines(file);

        foreach (var line in lines)
        {
            try
            {
                var split = line.Split(':');
                var entry = new HostEntry(split[1], int.Parse(split[2]));

                // If we already added an entry for this domain, add it to the list
                if (hosts.TryGetValue(split[0], out var entries))
                {
                    entries.Add(entry);
                }
                else
                {
                    hosts[split[0]] = [entry];
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
            hosts[domain] = [server];

            try
            {
                EnsureFileExists(file);
                File.AppendAllText(file, $"{domain}:{server.Host}:{server.Port}{Environment.NewLine}");
            }
            catch
            {
                // ignored
            }
        }

        return Task.CompletedTask;
    }

    private static void EnsureFileExists(string file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        if (!File.Exists(file))
        {
            File.WriteAllText(file, string.Empty);
        }
    }
}
