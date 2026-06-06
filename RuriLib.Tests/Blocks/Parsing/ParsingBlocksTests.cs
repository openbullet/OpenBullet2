using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils.Mockup;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;
using ParsingMethods = RuriLib.Blocks.Parsing.Methods;

namespace RuriLib.Tests.Blocks.Parsing;

public class ParsingBlocksTests
{
    [Fact]
    public void ParseBetweenStringsRecursive_UrlEncodeOutput_EncodesValues()
    {
        var data = NewBotData();

        var parsed = ParsingMethods.ParseBetweenStringsRecursive(
            data,
            "a[href='hello world'] b[href='two/three']",
            "[href='",
            "']",
            urlEncodeOutput: true);

        Assert.Equal(["hello%20world", "two%2Fthree"], parsed);
    }

    [Fact]
    public void ParseBetweenStrings_EmptyLeftDelim_ParsesFromStringStart()
    {
        var data = NewBotData();

        var parsed = ParsingMethods.ParseBetweenStrings(
            data,
            "The cat is on the table",
            string.Empty,
            " is");

        Assert.Equal("The cat", parsed);
    }

    [Fact]
    public void ParseBetweenStrings_EmptyRightDelim_ParsesToStringEnd()
    {
        var data = NewBotData();

        var parsed = ParsingMethods.ParseBetweenStrings(
            data,
            "The cat is on the table",
            "is ",
            string.Empty);

        Assert.Equal("on the table", parsed);
    }

    [Fact]
    public void ParseBetweenStrings_NullDelimiters_TreatsThemAsStringBounds()
    {
        var data = NewBotData();

        var parsed = ParsingMethods.ParseBetweenStrings(
            data,
            "The cat is on the table",
            null!,
            null!);

        Assert.Equal("The cat is on the table", parsed);
    }

    [Fact]
    public void QueryJsonTokenRecursive_UrlEncodeOutput_EncodesValues()
    {
        var data = NewBotData();

        var parsed = ParsingMethods.QueryJsonTokenRecursive(
            data,
            "{\"value\":\"hello world\"}",
            "value",
            urlEncodeOutput: true);

        Assert.Equal(["hello%20world"], parsed);
    }

    [Fact]
    public void QueryJsonToken_IntegerValue_ReturnsStringValue()
    {
        var data = NewBotData();

        var parsed = ParsingMethods.QueryJsonToken(
            data,
            "{\"user_id\":123,\"username\":\"alice\"}",
            "user_id");

        Assert.Equal("123", parsed);
    }

    [Fact]
    public void QueryJsonToken_LeadingWhitespaceJson_ReturnsValue()
    {
        var data = NewBotData();

        var parsed = ParsingMethods.QueryJsonToken(
            data,
            " \r\n\t{\"user_id\":123,\"username\":\"alice\"}",
            "user_id");

        Assert.Equal("123", parsed);
    }

    [Fact]
    public void MatchRegexGroupsRecursive_UrlEncodeOutput_EncodesValues()
    {
        var data = NewBotData();

        var parsed = ParsingMethods.MatchRegexGroupsRecursive(
            data,
            "url=hello world",
            "url=(.*)",
            "[1]",
            multiLine: false,
            urlEncodeOutput: true);

        Assert.Equal(["hello%20world"], parsed);
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
