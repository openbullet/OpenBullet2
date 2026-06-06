using System;
using RuriLib.Functions.Networking;
using RuriLib.Logging;
using RuriLib.Blocks.Requests.Dns;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;

namespace RuriLib.Tests.Blocks.Requests;

public class DnsRequestBlocksTests
{
    [Fact]
    public async Task DnsLookup_DnsOverHttps_UsesCustomEndpoint()
    {
        const string payload = "{\"Answer\":[{\"data\":\"10 mx01.example.com.\"},{\"data\":\"20 mx02.example.com.\"}]}";
        await using var server = LocalHttpResponseServer.CreateDelayed(TimeSpan.Zero, Encoding.UTF8.GetBytes(payload),
            "Content-Type: application/json");
        var data = NewBotData();

        var result = await Methods.LookupDnsAsync(
            data,
            "example.com",
            DnsRecordType.MX,
            DnsTransportProtocol.DnsOverHttps,
            server.Uri.ToString(),
            timeoutMilliseconds: 3000);

        Assert.Equal(["mx01.example.com", "mx02.example.com"], result);
    }

    [Fact]
    public async Task DnsLookup_Udp_UsesCustomServer()
    {
        await using var server = TestDnsServer.CreateARecord(IPAddress.Parse("127.0.0.42"));
        var data = NewBotData();

        var result = await Methods.LookupDnsAsync(
            data,
            "example.com",
            DnsRecordType.A,
            DnsTransportProtocol.Udp,
            $"{server.Host}:{server.UdpPort}",
            timeoutMilliseconds: 3000);

        Assert.Equal(["127.0.0.42"], result);
    }

    [Fact]
    public async Task DnsLookup_Tcp_UsesCustomServer()
    {
        await using var server = TestDnsServer.CreateARecord(IPAddress.Parse("127.0.0.43"));
        var data = NewBotData();

        var result = await Methods.LookupDnsAsync(
            data,
            "example.com",
            DnsRecordType.A,
            DnsTransportProtocol.Tcp,
            $"{server.Host}:{server.TcpPort}",
            timeoutMilliseconds: 3000);

        Assert.Equal(["127.0.0.43"], result);
    }

    private static BotData NewBotData()
        => new(
            new BotProviders(null!),
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("", new WordlistType()));
}
