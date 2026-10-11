using System.Collections.Generic;
using System.Dynamic;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Tests.Utils.Mockup;
using Xunit;

namespace RuriLib.Tests.Models.Bots;

public class ScriptGlobalsTests
{
    [Fact]
    public void Constructor_PopulatesInputSlices()
    {
        dynamic globals = new ExpandoObject();
        var botData = NewBotData(new DataLine("hello:world", new WordlistType
        {
            Separator = ":",
            Slices = ["left", "right"]
        }));

        var scriptGlobals = new ScriptGlobals(botData, globals);
        var input = (IDictionary<string, object?>)scriptGlobals.input;

        Assert.Equal("hello", input["left"]);
        Assert.Equal("world", input["right"]);
        Assert.Same(globals, scriptGlobals.globals);
    }

    [Fact]
    public void Constructor_UrlEncodesSlices_WhenConfigured()
    {
        dynamic globals = new ExpandoObject();
        var configSettings = new ConfigSettings();
        configSettings.DataSettings.UrlEncodeDataAfterSlicing = true;
        var providers = new global::RuriLib.Models.Bots.Providers(null!)
        {
            ProxySettings = new MockedProxySettingsProvider(),
            Security = new MockedSecurityProvider()
        };
        var botData = new BotData(
            providers,
            configSettings,
            new BotLogger(),
            new DataLine("hello world", new WordlistType
            {
                Separator = string.Empty,
                Slices = ["value"]
            }));

        var scriptGlobals = new ScriptGlobals(botData, globals);
        var input = (IDictionary<string, object?>)scriptGlobals.input;

        Assert.Equal("hello%20world", input["value"]);
    }

    [Fact]
    public void Constructor_PopulatesProxyVariables()
    {
        dynamic globals = new ExpandoObject();
        var proxy = new Proxy("1.2.3.4", 8080, ProxyType.Socks5, "user", "pass")
        {
            Country = "Italy",
            Ping = 120
        };
        var botData = NewBotData(NewLine(), proxy, useProxy: true);

        var proxyFields = GetProxyFields(new ScriptGlobals(botData, globals));

        Assert.Equal("1.2.3.4", proxyFields["ip"]);
        Assert.Equal("1.2.3.4", proxyFields["host"]);
        Assert.Equal(8080, proxyFields["port"]);
        Assert.Equal("user", proxyFields["username"]);
        Assert.Equal("pass", proxyFields["password"]);
        Assert.Equal("socks5", proxyFields["type"]);
        Assert.Equal("Italy", proxyFields["country"]);
        Assert.Equal(120, proxyFields["ping"]);
        Assert.Equal(true, proxyFields["inUse"]);
    }

    [Fact]
    public void Constructor_ProxyVariables_AreEmpty_WhenNoProxyAssigned()
    {
        dynamic globals = new ExpandoObject();

        var proxyFields = GetProxyFields(new ScriptGlobals(NewBotData(NewLine()), globals));

        Assert.Equal(string.Empty, proxyFields["ip"]);
        Assert.Equal(string.Empty, proxyFields["host"]);
        Assert.Equal(0, proxyFields["port"]);
        Assert.Equal(string.Empty, proxyFields["username"]);
        Assert.Equal(string.Empty, proxyFields["password"]);
        Assert.Equal(string.Empty, proxyFields["type"]);
        Assert.Equal(string.Empty, proxyFields["country"]);
        Assert.Equal(0, proxyFields["ping"]);
        Assert.Equal(false, proxyFields["inUse"]);
    }

    [Fact]
    public void Constructor_ProxyVariables_AreExposedButNotInUse_WhenJobHasProxiesDisabled()
    {
        dynamic globals = new ExpandoObject();
        var proxy = new Proxy("1.2.3.4", 8080);
        var botData = NewBotData(NewLine(), proxy, useProxy: false);

        var proxyFields = GetProxyFields(new ScriptGlobals(botData, globals));

        // The fields stay readable so configs can log them, but the request goes out directly
        Assert.Equal("1.2.3.4", proxyFields["ip"]);
        Assert.Equal(false, proxyFields["inUse"]);
    }

    [Fact]
    public void Constructor_SliceNamedProxy_TakesPrecedence()
    {
        dynamic globals = new ExpandoObject();
        var botData = NewBotData(
            new DataLine("fromWordlist", new WordlistType
            {
                Separator = string.Empty,
                Slices = ["proxy"]
            }),
            new Proxy("1.2.3.4", 8080),
            useProxy: true);

        var input = (IDictionary<string, object?>)new ScriptGlobals(botData, globals).input;

        // Existing configs slicing into "proxy" must keep working unchanged
        Assert.IsType<string>(input["proxy"]);
        Assert.Equal("fromWordlist", input["proxy"]);
    }

    private static DataLine NewLine()
        => new("hello:world", new WordlistType
        {
            Separator = ":",
            Slices = ["left", "right"]
        });

    private static IDictionary<string, object?> GetProxyFields(ScriptGlobals scriptGlobals)
    {
        var input = (IDictionary<string, object?>)scriptGlobals.input;
        Assert.True(input.ContainsKey("proxy"));

        return Assert.IsAssignableFrom<IDictionary<string, object?>>(input["proxy"]);
    }

    private static BotData NewBotData(DataLine line, Proxy? proxy = null, bool useProxy = false)
        => new(
            new global::RuriLib.Models.Bots.Providers(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            line,
            proxy,
            useProxy);
}
