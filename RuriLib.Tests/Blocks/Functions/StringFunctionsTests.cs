using RuriLib.Blocks.Functions.String;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;

namespace RuriLib.Tests.Blocks.Functions;

public class StringFunctionsTests
{
    [Fact]
    public void Translate_ReplaceOne_PrefersLongestMatch()
    {
        var data = NewBotData();
        var translations = new Dictionary<string, string>
        {
            ["ab"] = "X",
            ["a"] = "Y"
        };

        var translated = Methods.Translate(data, "ab", translations, replaceOne: true);

        Assert.Equal("X", translated);
    }

    [Fact]
    public void BasicStringOperations_ReturnExpectedValues()
    {
        var data = NewBotData();

        Assert.Equal(2, Methods.CountOccurrences(data, "banana", "na"));
        Assert.Equal("cba", Methods.Reverse(data, "abc"));
        Assert.Equal("ABC", Methods.ToUppercase(data, "abc"));
        Assert.Equal("abc", Methods.ToLowercase(data, "ABC"));
        Assert.Equal("middle", Methods.RegexReplace(data, "prefix-middle-suffix", @"^prefix-(.*)-suffix$", "$1"));
        Assert.Equal("b", Methods.CharAt(data, "abc", 1));
    }

    [Fact]
    public void HtmlEntities_And_Unescape_ReturnExpectedValues()
    {
        var data = NewBotData();

        var encoded = Methods.EncodeHTMLEntities(data, "<span>Tom & Jerry</span>");
        var decoded = Methods.DecodeHTMLEntities(data, encoded);
        var unescaped = Methods.Unescape(data, @"line1\nline2");

        Assert.Equal("&lt;span&gt;Tom &amp; Jerry&lt;/span&gt;", encoded);
        Assert.Equal("<span>Tom & Jerry</span>", decoded);
        Assert.Equal("line1\nline2", unescaped);
    }

    [Fact]
    public void UrlEncode_LongInput_RoundTrips()
    {
        var data = NewBotData();
        var input = new string('a', 3000) + " ?=+" + new string('b', 3000);

        var encoded = Methods.UrlEncode(data, input);
        var decoded = Methods.UrlDecode(data, encoded);

        Assert.Equal(input, decoded);
    }

    [Fact]
    public void RandomString_CustomCharset_UsesProvidedCharacters()
    {
        var data = NewBotData();

        var result = Methods.RandomString(data, "?c?c?c", "Z");

        Assert.Equal("ZZZ", result);
    }

    [Fact]
    public void RandomString_MixedMask_PreservesLiteralsAndTokenRules()
    {
        var data = NewBotData();

        var result = Methods.RandomString(data, "A?l?u?d?h?H?c??x?", "Z");

        Assert.Matches(new Regex(@"^A[a-z][A-Z][0-9][0-9a-f][0-9A-F]Z\?\?x\?$"), result);
    }

    [Fact]
    public void RandomString_QuantifiedMask_RepeatsToken()
    {
        var data = NewBotData();

        var result = Methods.RandomString(data, "?h{10}");

        Assert.Matches(new Regex("^[0-9a-f]{10}$"), result);
    }

    [Fact]
    public void RandomString_InvalidQuantifier_FallsBackToLiteralBraces()
    {
        var data = NewBotData();

        var result = Methods.RandomString(data, "?h{abc}");

        Assert.Matches(new Regex(@"^[0-9a-f]\{abc\}$"), result);
    }

    [Fact]
    public void GenerateGuid_V4_DefaultFormat_ReturnsVersion4Guid()
    {
        var data = NewBotData();

        var result = Methods.GenerateGuid(data);

        Assert.Matches(new Regex("^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$"), result);
    }

    [Fact]
    public void GenerateGuid_V7_CompactFormat_ReturnsVersion7Guid()
    {
        var data = NewBotData();

        var result = Methods.GenerateGuid(data, GuidVersion.V7, GuidFormat.N);

        Assert.Matches(new Regex("^[0-9a-f]{12}7[0-9a-f]{19}$"), result);
    }

    [Fact]
    public void GenerateGuid_BracesFormat_WrapsGuid()
    {
        var data = NewBotData();

        var result = Methods.GenerateGuid(data, GuidVersion.V4, GuidFormat.B);

        Assert.Matches(new Regex("^\\{[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}\\}$"), result);
    }

    [Fact]
    public void Split_Separator_ReturnsItems()
    {
        var data = NewBotData();

        var split = Methods.Split(data, "a:b:c", ":");

        Assert.Equal(["a", "b", "c"], split);
    }

    private static BotData NewBotData()
        => new(
            new BotProviders(null!)
            {
                RNG = new DeterministicRngProvider(),
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
}
