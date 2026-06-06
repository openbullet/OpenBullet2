using DnsClient;
using DnsClient.Protocol;
using Newtonsoft.Json.Linq;
using RuriLib.Functions.Http;
using RuriLib.Http.Models;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Functions.Networking;

/// <summary>
/// Performs DNS lookups using DNS over HTTPS.
/// </summary>
public static class DnsLookup
{
    private const string GoogleResolveEndpoint = "https://dns.google/resolve";

    /// <summary>
    /// Retrieves a list of records from Google's DNS over HTTP service at dns.google.com.
    /// The list is ordered by priority.
    /// </summary>
    /// <param name="domain">The domain name to resolve.</param>
    /// <param name="type">The record type to query.</param>
    /// <param name="proxy">An optional proxy to use for the HTTP request.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ordered list of record values.</returns>
    public static async Task<List<string>> FromGoogleAsync(
        string domain,
        string type,
        Proxy? proxy = null,
        int timeout = 30000,
        CancellationToken cancellationToken = default)
        => await FromDnsOverHttpsAsync(domain, type, GoogleResolveEndpoint, proxy, timeout, cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Retrieves a list of records from a DNS over HTTPS resolve endpoint.
    /// The list is ordered by priority.
    /// </summary>
    /// <param name="domain">The domain name to resolve.</param>
    /// <param name="type">The record type to query.</param>
    /// <param name="endpoint">The absolute DoH endpoint URL.</param>
    /// <param name="proxy">An optional proxy to use for the HTTP request.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ordered list of record values.</returns>
    public static async Task<List<string>> FromDnsOverHttpsAsync(
        string domain,
        string type,
        string? endpoint = null,
        Proxy? proxy = null,
        int timeout = 30000,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domain);
        ArgumentNullException.ThrowIfNull(type);

        endpoint = string.IsNullOrWhiteSpace(endpoint) ? GoogleResolveEndpoint : endpoint.Trim();

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out _))
        {
            throw new ArgumentException("The DNS over HTTPS endpoint must be an absolute URL", nameof(endpoint));
        }

        var separator = endpoint.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        var url = $"{endpoint}{separator}name={Uri.EscapeDataString(domain)}&type={Uri.EscapeDataString(type)}";

        using var httpClient = HttpFactory.GetRLHttpClient(proxy, new HttpOptions
        {
            ConnectTimeout = TimeSpan.FromMilliseconds(timeout),
            ReadWriteTimeout = TimeSpan.FromMilliseconds(timeout)
        });

        using var request = new HttpRequest
        {
            Uri = new Uri(url)
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var json = await (response.Content?.ReadAsStringAsync(cancellationToken)
            ?? Task.FromResult(string.Empty));
        var answers = JObject.Parse(json)["Answer"] as JArray;

        if (answers is null)
        {
            return [];
        }

        return answers
            .Select(ParseAnswer)
            .Where(record => record.HasValue)
            .Select(record => record!.Value)
            .OrderBy(record => record.Priority)
            .Select(record => record.Value)
            .ToList();
    }

    /// <summary>
    /// Retrieves a list of records from a DNS server using UDP or TCP.
    /// </summary>
    /// <param name="domain">The domain name to resolve.</param>
    /// <param name="type">The record type to query.</param>
    /// <param name="server">The optional DNS server, as host/IP or host/IP:port.</param>
    /// <param name="transport">The transport protocol.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ordered list of record values.</returns>
    public static async Task<List<string>> FromNameServerAsync(
        string domain,
        DnsRecordType type,
        string? server = null,
        DnsTransportProtocol transport = DnsTransportProtocol.Udp,
        int timeout = 30000,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domain);

        if (transport == DnsTransportProtocol.DnsOverHttps)
        {
            throw new ArgumentException("Use DNS over HTTPS specific helpers for DoH lookups", nameof(transport));
        }

        var options = string.IsNullOrWhiteSpace(server)
            ? new LookupClientOptions()
            : new LookupClientOptions(CreateNameServers(server).ToArray());
        options.UseCache = false;
        options.Timeout = TimeSpan.FromMilliseconds(timeout);
        options.Retries = 0;
        options.AutoResolveNameServers = string.IsNullOrWhiteSpace(server);
        options.UseTcpOnly = transport == DnsTransportProtocol.Tcp;
        options.UseTcpFallback = transport == DnsTransportProtocol.Udp;

        var lookup = new LookupClient(options);
        var response = await lookup.QueryAsync(domain, ToQueryType(type), cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.Answers
            .Select(FormatAnswer)
            .Where(answer => !string.IsNullOrWhiteSpace(answer))
            .ToList();
    }

    private static (int Priority, string Value)? ParseAnswer(JToken token)
    {
        var data = token.Value<string>("data");

        if (string.IsNullOrWhiteSpace(data))
        {
            return null;
        }

        var parts = data.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2 && int.TryParse(parts[0], out var priority))
        {
            return (priority, parts[1].TrimEnd('.'));
        }

        return (int.MaxValue, data.TrimEnd('.'));
    }

    private static QueryType ToQueryType(DnsRecordType type)
        => type switch
        {
            DnsRecordType.A => QueryType.A,
            DnsRecordType.AAAA => QueryType.AAAA,
            DnsRecordType.CAA => QueryType.CAA,
            DnsRecordType.CNAME => QueryType.CNAME,
            DnsRecordType.MX => QueryType.MX,
            DnsRecordType.NAPTR => QueryType.NAPTR,
            DnsRecordType.NS => QueryType.NS,
            DnsRecordType.PTR => QueryType.PTR,
            DnsRecordType.SOA => QueryType.SOA,
            DnsRecordType.SRV => QueryType.SRV,
            DnsRecordType.TLSA => QueryType.TLSA,
            DnsRecordType.TXT => QueryType.TXT,
            DnsRecordType.URI => QueryType.URI,
            _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(DnsRecordType))
        };

    private static IReadOnlyCollection<NameServer> CreateNameServers(string? server)
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            return [];
        }

        var trimmed = server.Trim();

        if (Uri.TryCreate($"dns://{trimmed}", UriKind.Absolute, out var endpointUri)
            && !string.IsNullOrWhiteSpace(endpointUri.Host))
        {
            var port = endpointUri.IsDefaultPort ? 53 : endpointUri.Port;
            var address = ResolveNameServerAddress(endpointUri.Host);
            return [new NameServer(address, port)];
        }

        return [new NameServer(ResolveNameServerAddress(trimmed))];
    }

    private static string FormatAnswer(DnsResourceRecord answer)
        => answer switch
        {
            ARecord a => a.Address.ToString(),
            AaaaRecord aaaa => aaaa.Address.ToString(),
            CNameRecord cname => cname.CanonicalName.Value.TrimEnd('.'),
            CaaRecord caa => $"{caa.Tag} {caa.Value}",
            MxRecord mx => mx.Exchange.Value.TrimEnd('.'),
            NsRecord ns => ns.NSDName.Value.TrimEnd('.'),
            PtrRecord ptr => ptr.PtrDomainName.Value.TrimEnd('.'),
            SoaRecord soa => $"{soa.MName.Value.TrimEnd('.')} {soa.RName.Value.TrimEnd('.')}",
            SrvRecord srv => $"{srv.Target.Value.TrimEnd('.')}:{srv.Port}",
            TlsaRecord tlsa => $"{tlsa.CertificateUsage} {tlsa.Selector} {tlsa.MatchingType} {tlsa.CertificateAssociationDataAsString}",
            TxtRecord txt => string.Concat(txt.Text),
            UriRecord uri => uri.Target,
            NAPtrRecord naptr => $"{naptr.Order} {naptr.Preference} {naptr.Flags} {naptr.Services} {naptr.RegularExpression} {naptr.Replacement.Value.TrimEnd('.')}".Trim(),
            _ => answer.ToString()
        };

    private static IPAddress ResolveNameServerAddress(string hostOrAddress)
    {
        if (IPAddress.TryParse(hostOrAddress, out var ipAddress))
        {
            return ipAddress;
        }

        return Dns.GetHostAddresses(hostOrAddress)[0];
    }
}
