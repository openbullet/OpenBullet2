using RuriLib.Attributes;
using RuriLib.Exceptions;
using RuriLib.Functions.Networking;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Requests.Dns;

/// <summary>
/// Blocks for performing DNS lookups.
/// </summary>
[BlockCategory("DNS", "Blocks to query DNS records", "#87ceeb")]
public static class Methods
{
    /// <summary>
    /// Queries DNS records for a given name.
    /// </summary>
    [Block("Queries DNS records for a given name", name = "DNS Lookup", id = "DnsLookup",
        aliases = new[] { "LookupDns", "LookupDnsAsync" })]
    public static async Task<List<string>> LookupDnsAsync(
        BotData data,
        string query,
        DnsRecordType recordType = DnsRecordType.A,
        DnsTransportProtocol transport = DnsTransportProtocol.DnsOverHttps,
        [BlockParam("Server", "For UDP/TCP use host, IP or host:port. For DoH use an absolute resolve endpoint URL. Leave empty to use the default resolver.")] string server = "",
        int timeoutMilliseconds = 10000)
    {
        data.Logger.LogHeader();

        List<string> answers;

        if (transport == DnsTransportProtocol.DnsOverHttps)
        {
            answers = await RuriLib.Functions.Networking.DnsLookup
                .FromDnsOverHttpsAsync(
                    query,
                    recordType.ToString(),
                    server,
                    data.UseProxy ? data.Proxy : null,
                    timeoutMilliseconds,
                    data.CancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            if (data.UseProxy)
            {
                throw new BlockExecutionException("UDP and TCP DNS lookups do not support proxies");
            }

            answers = await RuriLib.Functions.Networking.DnsLookup
                .FromNameServerAsync(
                    query,
                    recordType,
                    server,
                    transport,
                    timeoutMilliseconds,
                    data.CancellationToken)
                .ConfigureAwait(false);
        }

        data.Logger.Log($"Queried {recordType} records for {query} via {transport}", LogColors.SteelBlue);
        data.Logger.Log(answers, LogColors.SteelBlue);

        return answers;
    }
}
