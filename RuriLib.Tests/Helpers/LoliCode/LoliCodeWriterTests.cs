using System.Collections.Generic;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using Xunit;

namespace RuriLib.Tests.Helpers.LoliCode;

public class LoliCodeWriterTests
{
    [Fact]
    public void GetSettingValue_InterpolatedDictionary_SerializesEntries()
    {
        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Interpolated,
            InterpolatedSetting = new InterpolatedDictionaryOfStringsSetting
            {
                Value = new Dictionary<string, string>
                {
                    ["aaa"] = "bbb"
                }
            }
        };

        Assert.Equal("${(\"aaa\", \"bbb\")}", LoliCodeWriter.GetSettingValue(setting));
    }

    [Fact]
    public void GetSettingValue_NullInterpolatedString_ReturnsEmptyLiteral()
    {
        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Interpolated,
            InterpolatedSetting = new InterpolatedStringSetting { Value = null }
        };

        Assert.Equal("$\"\"", LoliCodeWriter.GetSettingValue(setting));
    }

    [Fact]
    public void GetSettingValue_InterpolatedStringWithEscapedAngleBrackets_PreservesEscapes()
    {
        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Interpolated,
            InterpolatedSetting = new InterpolatedStringSetting { Value = "hello <<name>>" }
        };

        Assert.Equal("$\"hello <<name>>\"", LoliCodeWriter.GetSettingValue(setting));
    }

    [Fact]
    public void GetSettingValue_InterpolatedStringWithTripleAngleBrackets_PreservesEscapes()
    {
        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Interpolated,
            InterpolatedSetting = new InterpolatedStringSetting { Value = "hello <<<name>>>" }
        };

        Assert.Equal("$\"hello <<<name>>>\"", LoliCodeWriter.GetSettingValue(setting));
    }

    [Fact]
    public void AppendSetting_DefaultFixedValue_DoesNotWriteLine()
    {
        var parameter = new StringParameter("name", "hello");
        var setting = parameter.ToBlockSetting();
        using var writer = new LoliCodeWriter();

        writer.AppendSetting(setting, parameter);

        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void AppendSetting_PrintDefaults_WritesLine()
    {
        var parameter = new StringParameter("name", "hello");
        var setting = parameter.ToBlockSetting();
        using var writer = new LoliCodeWriter();

        writer.AppendSetting(setting, parameter, printDefaults: true);

        Assert.Equal($"  name = \"hello\"{writer.NewLine}", writer.ToString());
    }

    [Fact]
    public void AppendSetting_DefaultInterpolatedDictionary_DoesNotWriteLine()
    {
        var parameter = new DictionaryOfStringsParameter("headers",
            new Dictionary<string, string> { ["Accept"] = "*/*" }, SettingInputMode.Interpolated);
        var setting = parameter.ToBlockSetting();
        using var writer = new LoliCodeWriter();

        writer.AppendSetting(setting, parameter);

        Assert.Equal(string.Empty, writer.ToString());
    }
}
