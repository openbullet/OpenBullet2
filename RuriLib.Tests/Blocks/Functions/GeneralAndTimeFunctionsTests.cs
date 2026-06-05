using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Tests.Utils.Mockup;
using System;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;
using GeneralMethods = RuriLib.Blocks.Functions.Methods;
using TimeMethods = RuriLib.Blocks.Functions.Time.Methods;

namespace RuriLib.Tests.Blocks.Functions;

public class GeneralAndTimeFunctionsTests
{
    [Fact]
    public void RandomUserAgent_ReturnsGeneratedValue()
    {
        var data = NewBotData(new StubUaProvider("ua-mobile"));

        var result = GeneralMethods.RandomUserAgent(data, UAPlatform.Mobile);

        Assert.Equal("ua-mobile", result);
    }

    [Fact]
    public void RandomUserAgent_WhenProviderThrows_ReturnsFallback()
    {
        var data = NewBotData(new ThrowingUaProvider());

        var result = GeneralMethods.RandomUserAgent(data);

        Assert.Equal("NO_RANDOM_UA_FOUND", result);
    }

    [Fact]
    public void DateToUnixTime_ParsesAsLocalTime()
    {
        var data = NewBotData(new StubUaProvider("ua"));
        const string datetime = "2020-04-18:00-00-00";
        const string format = "yyyy-MM-dd:HH-mm-ss";
        var expected = (long)DateTime.ParseExact(datetime, format, null).ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        var parsed = TimeMethods.DateToUnixTimeLong(data, datetime, format);

        Assert.Equal(expected, parsed);
    }

    [Fact]
    public void DateToUnixTime_WithMillisecondsOutput_ParsesAsLocalTime()
    {
        var data = NewBotData(new StubUaProvider("ua"));
        const string datetime = "2020-04-18:00-00-00";
        const string format = "yyyy-MM-dd:HH-mm-ss";
        var expected = (long)DateTime.ParseExact(datetime, format, null).ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

        var parsed = TimeMethods.DateToUnixTimeLong(data, datetime, format, outputMilliseconds: true);

        Assert.Equal(expected, parsed);
    }

    [Fact]
    public void UnixTimeToDate_WithMillisecondsInput_FormatsExpectedValue()
    {
        var data = NewBotData(new StubUaProvider("ua"));

        var date = TimeMethods.UnixTimeToDate(data, 1587168000000L, "yyyy-MM-dd:HH-mm-ss", inputMilliseconds: true);

        Assert.Equal("2020-04-18:00-00-00", date);
    }

    [Fact]
    public void UnixTimeToIso8601_FormatsExpectedValue()
    {
        var data = NewBotData(new StubUaProvider("ua"));

        var iso = TimeMethods.UnixTimeToISO8601(data, 1587168000);

        Assert.Equal("2020-04-18T00:00:00.000Z", iso);
    }

    private static BotData NewBotData(IRandomUAProvider randomUaProvider)
        => new(
            new BotProviders(null!)
            {
                RNG = new DeterministicRngProvider(),
                RandomUA = randomUaProvider,
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()));

    private sealed class DeterministicRngProvider : IRNGProvider
    {
        public Random GetNew() => new(12345);
    }

    private sealed class StubUaProvider(string userAgent) : IRandomUAProvider
    {
        public int Total => 1;

        public string Generate() => userAgent;

        public string Generate(UAPlatform platform) => userAgent;
    }

    private sealed class ThrowingUaProvider : IRandomUAProvider
    {
        public int Total => 0;

        public string Generate() => throw new InvalidOperationException("No user agents");

        public string Generate(UAPlatform platform) => throw new InvalidOperationException("No user agents");
    }
}
