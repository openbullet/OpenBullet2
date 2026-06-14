using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils.Mockup;
using System;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;
using CryptoMethods = RuriLib.Blocks.Functions.Crypto.Methods;

namespace RuriLib.Tests.Functions.Crypto;

public class JwtTests
{
    private const string Token = "eyJhbGciOiJub25lIn0.eyJzdWIiOiIxMjMiLCJyb2xlIjoiYWRtaW4ifQ.";
    private const string Payload = "{\"sub\":\"123\",\"role\":\"admin\"}";

    [Fact]
    public void JwtDecode_ValidToken_ReturnsPayload()
    {
        var decoded = RuriLib.Functions.Crypto.Crypto.JwtDecode(Token);

        Assert.Equal(Payload, decoded);
    }

    [Fact]
    public void JwtDecode_MissingPayload_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => RuriLib.Functions.Crypto.Crypto.JwtDecode("invalid"));
    }

    [Fact]
    public void JwtDecodeBlock_ValidToken_ReturnsPayload()
    {
        var data = NewBotData();

        var decoded = CryptoMethods.JwtDecode(data, Token);

        Assert.Equal(Payload, decoded);
    }

    private static BotData NewBotData()
        => new(new BotProviders(null!)
        {
            ProxySettings = new MockedProxySettingsProvider(),
            Security = new MockedSecurityProvider()
        },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()));
}
