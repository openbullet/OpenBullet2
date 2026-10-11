using RuriLib.Functions.Http;
using RuriLib.Functions.Http.Options;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace RuriLib.Tests.Functions.Http;

public class HttpClientRequestHandlerTests
{
    [Theory]
    [InlineData(0, HttpLibrary.CurlImpersonate, true)]
    [InlineData(1, HttpLibrary.CurlImpersonate, false)]
    [InlineData(0, HttpLibrary.SystemNet, false)]
    public void ShouldCaptureCurlRequestHeaders_OnlyForCurlDebugger(
        int botNumber,
        HttpLibrary library,
        bool expected)
    {
        var data = NewBotData();
        data.BOTNUM = botNumber;
        var options = new StandardHttpRequestOptions
        {
            HttpLibrary = library
        };

        Assert.Equal(expected, HttpClientRequestHandler.ShouldCaptureCurlRequestHeaders(data, options));
    }

    [Theory]
    [InlineData("1.1", HttpVersionPolicy.RequestVersionOrLower)]
    [InlineData("2.0", HttpVersionPolicy.RequestVersionOrLower)]
    [InlineData("3.0", HttpVersionPolicy.RequestVersionExact)]
    public void GetVersionPolicy_ForHttp3_ForcesExactVersion(
        string version,
        HttpVersionPolicy expected)
    {
        Assert.Equal(expected, HttpClientRequestHandler.GetVersionPolicy(Version.Parse(version)));
    }

    [Fact]
    public void TryParseCookie_AttributesWithoutCookiePair_ReturnsFalse()
    {
        var parsed = HttpClientRequestHandler.TryParseCookie(
            "HttpOnly;Secure;SameSite=None;",
            out var cookieName,
            out var cookieValue);

        Assert.False(parsed);
        Assert.Null(cookieName);
        Assert.Null(cookieValue);
    }

    [Theory]
    // No proxy assigned at all: the block setting must be irrelevant, never an error
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    // Proxy assigned and the job uses it: only useProxy=false bypasses
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    public void ResolveProxy_BypassesOnlyWhenRequested(
        bool botUsesProxy, bool blockUseProxy, bool expectProxyUsed)
    {
        var proxy = new Proxy("1.2.3.4", 8080);
        var data = NewBotData(proxy, botUsesProxy);

        var resolved = HttpRequestHandler.ResolveProxy(data, new StandardHttpRequestOptions
        {
            UseProxy = blockUseProxy
        });

        Assert.Equal(expectProxyUsed, resolved is not null);
        Assert.Equal(expectProxyUsed ? proxy : null, resolved);

        // The notice fires only when a proxy was actually in use and the block opted out.
        // A job that simply has proxies disabled is not "bypassing" anything, so it stays quiet.
        var expectNotice = botUsesProxy && !blockUseProxy;
        Assert.Equal(expectNotice ? 1 : 0,
            data.Logger.Entries.Count(e => e.Message.Contains("[NO PROXY]", StringComparison.Ordinal)));
    }

    private static BotData NewBotData(Proxy? proxy = null, bool useProxy = false) => new(
        new(null!)
        {
            ProxySettings = new MockedProxySettingsProvider(),
            Security = new MockedSecurityProvider()
        },
        new ConfigSettings(),
        new BotLogger(),
        new DataLine("", new WordlistType()),
        proxy,
        useProxy);
}
