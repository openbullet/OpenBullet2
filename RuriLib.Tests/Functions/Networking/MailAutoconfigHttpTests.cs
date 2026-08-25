using RuriLib.Exceptions;
using RuriLib.Functions.Networking;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Tests.Utils;
using RuriLib.Tests.Utils.Mockup;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;

namespace RuriLib.Tests.Functions.Networking;

public class MailAutoconfigHttpTests
{
    [Fact]
    public async Task GetStringAsync_UseProxyDisabled_IgnoresAssignedProxy()
    {
        const string expected = "<clientConfig />";
        await using var server = LocalHttpResponseServer.CreateResponse(
            HttpStatusCode.OK, Encoding.UTF8.GetBytes(expected), "Content-Type: text/xml");
        var data = NewBotData();
        data.Proxy = new Proxy("127.0.0.1", 1);
        data.UseProxy = false;

        var result = await MailAutoconfigHttp.GetStringAsync(data, server.Uri.ToString());

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetStringAsync_NonSuccessStatus_ThrowsDescriptiveException()
    {
        await using var server = LocalHttpResponseServer.CreateResponse(
            HttpStatusCode.Forbidden, Encoding.UTF8.GetBytes("Forbidden"));
        var data = NewBotData();

        var exception = await Assert.ThrowsAsync<BlockExecutionException>(
            () => MailAutoconfigHttp.GetStringAsync(data, server.Uri.ToString()));

        Assert.Contains("HTTP 403 (Forbidden)", exception.Message);
    }

    private static BotData NewBotData()
        => new(
            new BotProviders(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()));
}
