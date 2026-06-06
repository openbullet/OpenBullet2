using System.Collections.Generic;
using System.Dynamic;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
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

    private static BotData NewBotData(DataLine line)
        => new(
            new global::RuriLib.Models.Bots.Providers(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            line);
}
