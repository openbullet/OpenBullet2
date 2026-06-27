using RuriLib.Functions.Http;
using RuriLib.Functions.Http.Options;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils.Mockup;
using System;
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

    private static BotData NewBotData() => new(
        new(null!)
        {
            ProxySettings = new MockedProxySettingsProvider(),
            Security = new MockedSecurityProvider()
        },
        new ConfigSettings(),
        new BotLogger(),
        new DataLine("", new WordlistType()));
}
