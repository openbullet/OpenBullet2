using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using Xunit;

namespace OpenBullet2.Console.Tests;

public class ConsoleRunPlannerTests
{
    [Fact]
    public void GetRunMode_WithoutSingleRunInput_ReturnsMultiRunJob()
    {
        var options = new ConsoleOptions
        {
            ConfigFile = "config.opk",
            WordlistFile = "wordlist.txt",
            WordlistType = "Default"
        };

        var runMode = ConsoleRunPlanner.GetRunMode(options);

        Assert.Equal(ConsoleRunMode.MultiRunJob, runMode);
    }

    [Fact]
    public void GetRunMode_WithSingleRunInput_ReturnsSingleRunDebug()
    {
        var options = new ConsoleOptions
        {
            ConfigFile = "config.opk",
            SingleRunData = "12345",
            WordlistType = "Default"
        };

        var runMode = ConsoleRunPlanner.GetRunMode(options);

        Assert.Equal(ConsoleRunMode.SingleRunDebug, runMode);
    }

    [Fact]
    public void Validate_SingleRunRejectsWordlistAndProxyFileOptions()
    {
        var options = new ConsoleOptions
        {
            ConfigFile = "config.opk",
            SingleRunData = "12345",
            WordlistFile = "wordlist.txt",
            ProxyFile = "proxies.txt",
            WordlistType = "Default"
        };

        var errors = ConsoleRunPlanner.Validate(options);

        Assert.Contains(errors, e => e.Contains("--wordlist", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("--proxies", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_MultiRunRequiresWordlistOrRange()
    {
        var options = new ConsoleOptions
        {
            ConfigFile = "config.opk",
            WordlistType = "Default"
        };

        var errors = ConsoleRunPlanner.Validate(options);

        Assert.Contains(errors, e => e.Contains("wordlist file", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ProxyWithoutSingleRunInput_ReturnsHelpfulError()
    {
        var options = new ConsoleOptions
        {
            ConfigFile = "config.opk",
            SingleRunProxy = "127.0.0.1:8080",
            WordlistFile = "wordlist.txt",
            WordlistType = "Default"
        };

        var errors = ConsoleRunPlanner.Validate(options);

        Assert.Contains(errors, e => e.Contains("--data", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildDebuggerOptions_MapsSingleRunSettings()
    {
        var options = new ConsoleOptions
        {
            ConfigFile = "config.opk",
            SingleRunData = "12345",
            SingleRunProxy = "127.0.0.1:8080",
            WordlistType = "Default",
            ProxyType = ProxyType.Socks5,
            StepByStep = true
        };

        var debuggerOptions = ConsoleRunPlanner.BuildDebuggerOptions(options);

        Assert.Equal("12345", debuggerOptions.TestData);
        Assert.Equal("Default", debuggerOptions.WordlistType);
        Assert.True(debuggerOptions.UseProxy);
        Assert.Equal("127.0.0.1:8080", debuggerOptions.TestProxy);
        Assert.Equal(ProxyType.Socks5, debuggerOptions.ProxyType);
        Assert.True(debuggerOptions.StepByStep);
    }

    [Fact]
    public void Validate_SingleRunRejectsBotsAndSkip()
    {
        var options = new ConsoleOptions
        {
            ConfigFile = "config.opk",
            SingleRunData = "12345",
            WordlistType = "Default",
            BotsNumber = 2,
            Skip = 5
        };

        var errors = ConsoleRunPlanner.Validate(options);

        Assert.Contains(errors, e => e.Contains("--bots", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("--skip", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(5, 2, 5)]
    [InlineData(0, 3, 3)]
    [InlineData(0, 0, 1)]
    [InlineData(-1, 1, 1)]
    public void ResolveBots_UsesRequestedOrSuggestedWithMinimumOne(int requestedBots, int suggestedBots, int expected)
    {
        var resolved = ConsoleRunPlanner.ResolveBots(requestedBots, suggestedBots);

        Assert.Equal(expected, resolved);
    }
}
