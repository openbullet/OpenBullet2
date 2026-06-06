using RuriLib.Helpers;
using Xunit;

namespace RuriLib.Tests.Helpers;

public class CommandLineArgumentParserTests
{
    [Fact]
    public void Parse_MultipleFlags_SplitsOnWhitespace()
    {
        var arguments = CommandLineArgumentParser.Parse("--disable-notifications --incognito");

        Assert.Equal(["--disable-notifications", "--incognito"], arguments);
    }

    [Fact]
    public void Parse_QuotedValueWithSpaces_PreservesQuotedValue()
    {
        var arguments = CommandLineArgumentParser.Parse("--user-agent=\"Mozilla/5.0 Test Agent\" --lang=en-US");

        Assert.Equal(["--user-agent=Mozilla/5.0 Test Agent", "--lang=en-US"], arguments);
    }

    [Fact]
    public void Parse_SingleQuotedValueWithSpaces_PreservesQuotedValue()
    {
        var arguments = CommandLineArgumentParser.Parse("--profile-directory='Profile 3' --start-maximized");

        Assert.Equal(["--profile-directory=Profile 3", "--start-maximized"], arguments);
    }

    [Fact]
    public void ParseMany_ConfigAndExtraArgs_MergesAllArguments()
    {
        var arguments = CommandLineArgumentParser.ParseMany(
            "--window-size=1920,1080 --user-agent=\"Mozilla/5.0 Test Agent\"",
            "--lang=en-US --proxy-bypass-list='*.internal.local'");

        Assert.Equal(
            ["--window-size=1920,1080", "--user-agent=Mozilla/5.0 Test Agent", "--lang=en-US", "--proxy-bypass-list=*.internal.local"],
            arguments);
    }

    [Fact]
    public void ParseMany_NullAndWhitespaceSegments_AreIgnored()
    {
        var arguments = CommandLineArgumentParser.ParseMany(null, "   ", "--disable-gpu");

        Assert.Equal(["--disable-gpu"], arguments);
    }

    [Fact]
    public void Parse_EscapedQuotesInsideQuotedValue_UnescapesQuote()
    {
        var arguments = CommandLineArgumentParser.Parse("--header=\"value \\\"with quote\\\"\" --flag");

        Assert.Equal(["--header=value \"with quote\"", "--flag"], arguments);
    }
}
