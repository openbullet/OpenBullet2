using RuriLib.Blocks.Requests.Http;
using RuriLib.Functions.Http;
using RuriLib.Functions.Http.Options;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Providers.Proxies;
using RuriLib.Tests.Utils;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Functions.Http;

public class SystemNetTimeoutTests
{
    [Fact]
    public async Task HttpRequestStandard_SystemNet_DoesNotUseProxyReadWriteTimeoutAsRequestTimeout()
    {
        await using var server = LocalHttpResponseServer.CreateDelayed(
            TimeSpan.FromMilliseconds(200),
            """{"ok":true}"""u8.ToArray(),
            "Content-Type: application/json");

        var data = NewBotData(TimeSpan.FromMilliseconds(50));
        var options = new StandardHttpRequestOptions
        {
            Url = server.Uri.ToString(),
            Method = HttpMethod.GET,
            HttpLibrary = HttpLibrary.SystemNet,
            TimeoutMilliseconds = 5000
        };

        await Methods.HttpRequestStandard(data, options);

        Assert.Equal(200, data.RESPONSECODE);
        Assert.Equal("""{"ok":true}""", data.SOURCE);
    }

    private static BotData NewBotData(TimeSpan readWriteTimeout) => new(
        new(null!)
        {
            ProxySettings = new FixedProxySettingsProvider(readWriteTimeout),
            Security = new MockedSecurityProvider()
        },
        new ConfigSettings(),
        new BotLogger(),
        new DataLine("", new WordlistType()),
        null,
        false);

    private sealed class FixedProxySettingsProvider(TimeSpan readWriteTimeout) : IProxySettingsProvider
    {
        public TimeSpan ConnectTimeout => TimeSpan.FromSeconds(10);

        public TimeSpan ReadWriteTimeout => readWriteTimeout;

        public bool ContainsBanKey(string text, out string matchedKey, bool caseSensitive = false)
        {
            matchedKey = string.Empty;
            return false;
        }

        public bool ContainsRetryKey(string text, out string matchedKey, bool caseSensitive = false)
        {
            matchedKey = string.Empty;
            return false;
        }
    }
}
