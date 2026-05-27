using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.HttpRequest;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Custom.Parse;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;
using Xunit;

namespace RuriLib.Tests.Helpers.Blocks;

public class BlockClonerTests
{
    [Fact]
    public void Clone_KeycheckBlock_ReturnsIndependentClone()
    {
        var block = BlockFactory.GetBlock<KeycheckBlockInstance>("Keycheck");
        block.Disabled = true;
        block.Label = "Check result";
        (block.Settings["banIfNoMatch"].FixedSetting as BoolSetting)!.Value = false;
        block.Keychains =
        [
            new()
            {
                ResultStatus = "SUCCESS",
                Mode = KeychainMode.OR,
                Keys =
                [
                    new StringKey
                    {
                        Comparison = StrComparison.Contains,
                        Left = BlockSettingFactory.CreateStringSetting(string.Empty, "data.SOURCE", SettingInputMode.Variable),
                        Right = BlockSettingFactory.CreateStringSetting(string.Empty, "abc")
                    }
                ]
            }
        ];

        var clone = BlockCloner.Clone(block);

        Assert.NotSame(block, clone);
        Assert.Equal(block.ToLC(printDefaultParams: true), clone.ToLC(printDefaultParams: true));
        Assert.NotSame(block.Keychains[0], clone.Keychains[0]);

        ((StringKey)clone.Keychains[0].Keys[0]).Right =
            BlockSettingFactory.CreateStringSetting(string.Empty, "changed");

        Assert.Equal("abc", Assert.IsType<StringSetting>(
            ((StringKey)block.Keychains[0].Keys[0]).Right.FixedSetting).Value);
    }

    [Fact]
    public void Clone_HttpRequestBlockWithEnumSetting_ReturnsIndependentClone()
    {
        var block = BlockFactory.GetBlock<HttpRequestBlockInstance>("HttpRequest");
        block.Safe = true;
        (block.Settings["url"].FixedSetting as StringSetting)!.Value = "https://example.com";
        (block.Settings["method"].FixedSetting as EnumSetting)!.Value = "POST";
        block.RequestParams = new StandardRequestParams
        {
            Content = BlockSettingFactory.CreateStringSetting(string.Empty, "name=value"),
            ContentType = BlockSettingFactory.CreateStringSetting(string.Empty, "application/x-www-form-urlencoded")
        };

        var clone = BlockCloner.Clone(block);

        Assert.NotSame(block, clone);
        Assert.Equal(block.ToLC(printDefaultParams: true), clone.ToLC(printDefaultParams: true));
        Assert.NotSame(block.RequestParams, clone.RequestParams);

        (clone.Settings["method"].FixedSetting as EnumSetting)!.Value = "GET";

        Assert.Equal("POST", Assert.IsType<EnumSetting>(block.Settings["method"].FixedSetting).Value);
    }

    [Fact]
    public void Clone_ParseBlock_ReturnsIndependentClone()
    {
        var block = BlockFactory.GetBlock<ParseBlockInstance>("Parse");
        block.Safe = true;
        block.Recursive = true;
        block.Mode = ParseMode.Regex;
        block.IsCapture = true;
        block.OutputVariable = "parsedOutput";
        block.Settings["input"].InputMode = SettingInputMode.Variable;
        block.Settings["input"].InputVariableName = "data.SOURCE";
        (block.Settings["pattern"].FixedSetting as StringSetting)!.Value = "abc(.*)";
        (block.Settings["outputFormat"].FixedSetting as StringSetting)!.Value = "$1";
        (block.Settings["multiLine"].FixedSetting as BoolSetting)!.Value = true;

        var clone = BlockCloner.Clone(block);

        Assert.NotSame(block, clone);
        Assert.Equal(block.ToLC(printDefaultParams: true), clone.ToLC(printDefaultParams: true));
        Assert.NotSame(block.Settings["pattern"], clone.Settings["pattern"]);

        (clone.Settings["pattern"].FixedSetting as StringSetting)!.Value = "changed";

        Assert.Equal("abc(.*)", Assert.IsType<StringSetting>(
            block.Settings["pattern"].FixedSetting).Value);
    }

    [Fact]
    public void Clone_LoliCodeBlock_ReturnsIndependentClone()
    {
        var block = new LoliCodeBlockInstance(new LoliCodeBlockDescriptor())
        {
            Disabled = true,
            Label = "Raw script",
            Script = "LOG \"hello\""
        };

        var clone = BlockCloner.Clone(block);

        Assert.NotSame(block, clone);
        Assert.Equal(block.Disabled, clone.Disabled);
        Assert.Equal(block.Label, clone.Label);
        Assert.Equal(block.Script, clone.Script);

        clone.Script = "LOG \"changed\"";

        Assert.Equal("LOG \"hello\"", block.Script);
    }
}
