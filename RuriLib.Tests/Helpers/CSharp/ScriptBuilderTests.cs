using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using RuriLib.Helpers.CSharp;
using RuriLib.Logging;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils.Mockup;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;

namespace RuriLib.Tests.Helpers.CSharp;

public class ScriptBuilderTests
{
    [Fact]
    public void Build_CustomUsingDirective_AddsParsedImport()
    {
        var settings = new ScriptSettings
        {
            CustomUsings =
            [
                "using System.Text;",
                " System.Globalization "
            ]
        };

        var script = new ScriptBuilder().Build("return 1;", settings, null!);

        Assert.Contains("System.Text", script.Options.Imports);
        Assert.Contains("System.Globalization", script.Options.Imports);
    }

    [Fact]
    public void GetUsings_DoesNotContainDuplicates()
    {
        var usings = ScriptBuilder.GetUsings().ToList();

        Assert.Equal(usings.Count, usings.Distinct().Count());
    }

    [Fact]
    public void GetUsings_ContainsCommonRawCSharpNamespaces()
    {
        var usings = ScriptBuilder.GetUsings().ToList();

        Assert.Contains("System.Globalization", usings);
        Assert.Contains("System.Text.Json", usings);
        Assert.Contains("System.Text.RegularExpressions", usings);
        Assert.Contains("RuriLib.Blocks.Puppeteer.Page.Methods", usings);
        Assert.Contains("RuriLib.Blocks.Browser.Page.Methods", usings);
    }

    [Fact]
    public async Task Build_DynamicHelperCallFromGlobals_RunsSuccessfully()
    {
        dynamic globals = new ExpandoObject();
        ((IDictionary<string, object?>)globals)["value"] = "hello";

        var scriptGlobals = new ScriptGlobals(
            new BotData(
                new BotProviders(null!)
                {
                    ProxySettings = new MockedProxySettingsProvider(),
                    Security = new MockedSecurityProvider()
                },
                new ConfigSettings(),
                new BotLogger(),
                new DataLine("input", new WordlistType())),
            globals);

        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Variable,
            InputVariableName = "globals.value",
            FixedSetting = new StringSetting()
        };

        var script = new ScriptBuilder().Build(
            $"return {CSharpWriter.FromSetting(setting)};",
            new ScriptSettings(),
            null!);

        var state = await script.RunAsync(scriptGlobals, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("hello", state.ReturnValue);
    }

    [Fact]
    public void Build_LegacyPuppeteerMethodCall_CompilesBecauseLegacyImportsArePreserved()
    {
        var script = new ScriptBuilder().Build(
            "PuppeteerGetCurrentUrl(data); return 1;",
            new ScriptSettings(),
            null!);

        Assert.Contains("RuriLib.Blocks.Puppeteer.Page.Methods", script.Options.Imports);
    }
}
