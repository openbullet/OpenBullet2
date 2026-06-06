using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.CSharp;
using RuriLib.Functions.Networking;
using RuriLib.Exceptions;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using Xunit;

namespace RuriLib.Tests.Models.Blocks;

public class AutoBlockInstanceTests
{
    private readonly string _nl = Environment.NewLine;

    [Fact]
    public void ToLC_ParseLRBlock_OutputScript()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
        block.OutputVariable = "myOutput";
        block.IsCapture = false;
        block.Disabled = true;
        block.Label = "My Label";
        var input = block.Settings["input"];
        var index = block.Settings["index"];
        var length = block.Settings["length"];

        input.InputMode = SettingInputMode.Variable;
        input.InputVariableName = "myInput";

        index.InputMode = SettingInputMode.Fixed;
        (index.FixedSetting as IntSetting)!.Value = 3;

        length.InputMode = SettingInputMode.Fixed;

        var expected = $"DISABLED{_nl}LABEL:My Label{_nl}  input = @myInput{_nl}  index = 3{_nl}  => VAR @myOutput{_nl}";
        Assert.Equal(expected, block.ToLC());
    }

    [Fact]
    public void FromLC_ParseLRBlock_BuildBlock()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
        var script = $"DISABLED{_nl}LABEL:My Label{_nl}  input = @myInput{_nl}  index = 3{_nl}  => CAP @myOutput{_nl}";
        var lineNumber = 0;
        block.FromLC(ref script, ref lineNumber);

        Assert.True(block.Disabled);
        Assert.Equal("My Label", block.Label);
        Assert.Equal("myOutput", block.OutputVariable);
        Assert.True(block.IsCapture);

        var input = block.Settings["input"];
        var index = block.Settings["index"];

        Assert.Equal(SettingInputMode.Variable, input.InputMode);
        Assert.Equal("myInput", input.InputVariableName);
        Assert.Equal(SettingInputMode.Fixed, index.InputMode);
        Assert.Equal(3, (index.FixedSetting as IntSetting)!.Value);
    }

    [Fact]
    public void FromLC_InvalidSetting_PreservesLineParsingDetails()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var script = $"  value{_nl}";
        var lineNumber = 0;

        var ex = Assert.Throws<LoliCodeParsingException>(() => block.FromLC(ref script, ref lineNumber));

        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(6, ex.ColumnNumber);
        Assert.IsType<LineParsingException>(ex.InnerException);
        Assert.Contains("Expected '=' after setting name 'value'", ex.Message);
    }

    [Fact]
    public void ToSyntax_SyncReturnValue_OutputScript()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
        block.OutputVariable = "myOutput";
        var input = block.Settings["input"];
        var index = block.Settings["index"];
        var length = block.Settings["length"];

        input.InputMode = SettingInputMode.Variable;
        input.InputVariableName = "myInput";

        index.InputMode = SettingInputMode.Fixed;
        (index.FixedSetting as IntSetting)!.Value = 3;

        length.InputMode = SettingInputMode.Fixed;
        (length.FixedSetting as IntSetting)!.Value = 5;

        var expected = $"string myOutput = Substring(data, myInput.AsString(), 3, 5);{_nl}data.LogVariableAssignment(nameof(myOutput));{_nl}";
        Assert.Equal(expected, RenderSyntax(block, []));
    }

    [Fact]
    public void ToSyntax_SyncReturnValueCapture_OutputScript()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
        block.OutputVariable = "myOutput";
        block.IsCapture = true;
        var input = block.Settings["input"];
        var index = block.Settings["index"];
        var length = block.Settings["length"];

        input.InputMode = SettingInputMode.Variable;
        input.InputVariableName = "myInput";

        index.InputMode = SettingInputMode.Fixed;
        (index.FixedSetting as IntSetting)!.Value = 3;

        length.InputMode = SettingInputMode.Fixed;
        (length.FixedSetting as IntSetting)!.Value = 5;

        var expected = $"string myOutput = Substring(data, myInput.AsString(), 3, 5);{_nl}data.LogVariableAssignment(nameof(myOutput));{_nl}data.MarkForCapture(nameof(myOutput));{_nl}";
        Assert.Equal(expected, RenderSyntax(block, []));
    }

    [Fact]
    public void ToSyntax_AsyncNoReturnValue_OutputScript()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("TcpConnect");
        var url = block.Settings["host"];
        var port = block.Settings["port"];
        var ssl = block.Settings["useSSL"];
        var timeout = block.Settings["timeoutMilliseconds"];

        (url.FixedSetting as StringSetting)!.Value = "example.com";
        (port.FixedSetting as IntSetting)!.Value = 80;
        (ssl.FixedSetting as BoolSetting)!.Value = false;
        (timeout.FixedSetting as IntSetting)!.Value = 1000;

        var expected = $"await TcpConnect(data, \"example.com\", 80, false, 1000).ConfigureAwait(false);{_nl}";
        Assert.Equal(expected, RenderSyntax(block, []));
    }

    [Fact]
    public void ToSyntax_BlockIdOverride_UsesAsyncMethodName()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("FileExists");
        block.OutputVariable = "exists";
        var path = block.Settings["path"];

        path.InputMode = SettingInputMode.Fixed;
        (path.FixedSetting as StringSetting)!.Value = "test.txt";

        var expected = $"bool exists = await FileExistsAsync(data, \"test.txt\").ConfigureAwait(false);{_nl}data.LogVariableAssignment(nameof(exists));{_nl}";
        Assert.Equal(expected, RenderSyntax(block, []));
    }

    [Fact]
    public void ToSyntax_TaskReturningWrapper_UsesAwait()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("BrowserOpen");

        var expected = $"await BrowserOpen(data, \"\").ConfigureAwait(false);{_nl}";
        Assert.Equal(expected, RenderSyntax(block, []));
    }

    [Fact]
    public void ToSyntax_SyncReturnValueAlreadyDeclared_OutputScript()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
        block.OutputVariable = "myOutput";
        var input = block.Settings["input"];
        var index = block.Settings["index"];
        var length = block.Settings["length"];

        input.InputMode = SettingInputMode.Interpolated;
        input.InterpolatedSetting = new InterpolatedStringSetting { Value = "my <interp> string" };

        index.InputMode = SettingInputMode.Fixed;
        (index.FixedSetting as IntSetting)!.Value = 3;

        length.InputMode = SettingInputMode.Variable;
        length.InputVariableName = "myLength";

        var expected = $"myOutput = Substring(data, $\"my {{interp}} string\", 3, myLength.AsInt());{_nl}data.LogVariableAssignment(nameof(myOutput));{_nl}";
        Assert.Equal(expected, RenderSyntax(block, ["myOutput"]));
    }

    [Fact]
    public void ToSyntax_SyncReturnValueEscapedAngleBrackets_OutputScript()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
        block.OutputVariable = "myOutput";
        var input = block.Settings["input"];
        var index = block.Settings["index"];
        var length = block.Settings["length"];

        input.InputMode = SettingInputMode.Interpolated;
        input.InterpolatedSetting = new InterpolatedStringSetting { Value = "hello <<<name>>> and <<friend>>" };

        index.InputMode = SettingInputMode.Fixed;
        (index.FixedSetting as IntSetting)!.Value = 3;

        length.InputMode = SettingInputMode.Variable;
        length.InputVariableName = "myLength";

        var expected = $"myOutput = Substring(data, $\"hello <{{name}}> and <friend>\", 3, myLength.AsInt());{_nl}data.LogVariableAssignment(nameof(myOutput));{_nl}";
        Assert.Equal(expected, RenderSyntax(block, ["myOutput"]));
    }

    [Fact]
    public void ToSyntax_TracksExpectedPatternsAndVariableFlow()
    {
        AssertSyntax(CreateSubstringBlock(), [],
            ["myOutput"],
            "string myOutput = Substring(data, myInput.AsString(), 3, 5);",
            "data.LogVariableAssignment(nameof(myOutput));");

        AssertSyntax(CreateSubstringBlock(isCapture: true), [],
            ["myOutput"],
            "data.MarkForCapture(nameof(myOutput));");

        AssertSyntax(CreateSubstringBlock(safe: true), [],
            ["myOutput"],
            "string myOutput = string.Empty;",
            "try",
            "catch (Exception safeException)");

        AssertSyntax(CreateSubstringBlock(outputVariable: "globals.sharedSlice", useGlobalsInput: true), [],
            [],
            "globals.sharedSlice = Substring(data, ObjectExtensions.DynamicAsString(globals.inputValue), 3, 5);",
            "data.LogVariableAssignment(nameof(globals.sharedSlice));");

        AssertSyntax(CreateSubstringBlock(alreadyDeclared: true, interpolatedInput: "hello <name>"), ["myOutput"],
            ["myOutput"],
            "myOutput = Substring(data, $\"hello {name}\", 3, ObjectExtensions.DynamicAsInt(input.length));",
            "data.LogVariableAssignment(nameof(myOutput));");

        AssertSyntax(CreateFileExistsBlock(), [],
            ["exists"],
            "bool exists = await FileExistsAsync(data, ObjectExtensions.DynamicAsString(globals.filePath)).ConfigureAwait(false);");

        AssertSyntax(CreateFileExistsBlock(safe: true, outputVariable: "globals.fileExists"), [],
            [],
            "globals.fileExists = await FileExistsAsync(data, ObjectExtensions.DynamicAsString(globals.filePath)).ConfigureAwait(false);",
            "data.LogVariableAssignment(nameof(globals.fileExists));",
            "catch (Exception safeException)");

        AssertSyntax(CreateTcpConnectBlock(), [],
            [],
            "await TcpConnect(data, ObjectExtensions.DynamicAsString(globals.host), 80, false, ObjectExtensions.DynamicAsInt(input.timeout)).ConfigureAwait(false);");

        AssertSyntax(CreateDnsLookupBlock(), [],
            ["dnsAnswers"],
            "LookupDnsAsync(data, ObjectExtensions.DynamicAsString(globals.query)",
            "\"127.0.0.1:5353\"",
            "1500).ConfigureAwait(false);",
            "data.LogVariableAssignment(nameof(dnsAnswers));");
    }

    private static AutoBlockInstance CreateSubstringBlock(
        bool safe = false,
        bool isCapture = false,
        bool alreadyDeclared = false,
        string outputVariable = "myOutput",
        bool useGlobalsInput = false,
        string? interpolatedInput = null)
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
        block.Safe = safe;
        block.IsCapture = isCapture;
        block.OutputVariable = outputVariable;

        var input = block.Settings["input"];
        var index = block.Settings["index"];
        var length = block.Settings["length"];

        if (interpolatedInput is not null)
        {
            input.InputMode = SettingInputMode.Interpolated;
            input.InterpolatedSetting = new InterpolatedStringSetting { Value = interpolatedInput };
        }
        else
        {
            input.InputMode = SettingInputMode.Variable;
            input.InputVariableName = useGlobalsInput ? "globals.inputValue" : "myInput";
        }

        index.InputMode = SettingInputMode.Fixed;
        (index.FixedSetting as IntSetting)!.Value = 3;

        if (alreadyDeclared)
        {
            length.InputMode = SettingInputMode.Variable;
            length.InputVariableName = "input.length";
        }
        else
        {
            length.InputMode = SettingInputMode.Fixed;
            (length.FixedSetting as IntSetting)!.Value = 5;
        }

        return block;
    }

    private static AutoBlockInstance CreateFileExistsBlock(bool safe = false, string outputVariable = "exists")
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("FileExists");
        block.Safe = safe;
        block.OutputVariable = outputVariable;

        var path = block.Settings["path"];
        path.InputMode = SettingInputMode.Variable;
        path.InputVariableName = "globals.filePath";

        return block;
    }

    private static AutoBlockInstance CreateTcpConnectBlock()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("TcpConnect");
        var url = block.Settings["host"];
        var port = block.Settings["port"];
        var ssl = block.Settings["useSSL"];
        var timeout = block.Settings["timeoutMilliseconds"];

        url.InputMode = SettingInputMode.Variable;
        url.InputVariableName = "globals.host";
        (port.FixedSetting as IntSetting)!.Value = 80;
        (ssl.FixedSetting as BoolSetting)!.Value = false;
        timeout.InputMode = SettingInputMode.Variable;
        timeout.InputVariableName = "input.timeout";

        return block;
    }

    private static AutoBlockInstance CreateDnsLookupBlock()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("DnsLookup");
        block.OutputVariable = "dnsAnswers";

        var query = block.Settings["query"];
        var recordType = block.Settings["recordType"];
        var transport = block.Settings["transport"];
        var server = block.Settings["server"];
        var timeout = block.Settings["timeoutMilliseconds"];

        query.InputMode = SettingInputMode.Variable;
        query.InputVariableName = "globals.query";
        (recordType.FixedSetting as EnumSetting)!.Value = nameof(DnsRecordType.MX);
        (transport.FixedSetting as EnumSetting)!.Value = nameof(DnsTransportProtocol.Tcp);
        (server.FixedSetting as StringSetting)!.Value = "127.0.0.1:5353";
        (timeout.FixedSetting as IntSetting)!.Value = 1500;

        return block;
    }

    private static void AssertSyntax(
        BlockInstance block,
        List<string> definedVariables,
        List<string> expectedDefinedVariables,
        params string[] expectedFragments)
    {
        var syntaxVariables = new List<string>(definedVariables);
        var syntax = block.ToSyntax(new BlockSyntaxGenerationContext(syntaxVariables, new ConfigSettings())).ToSnippet();

        foreach (var expectedFragment in expectedFragments)
        {
            Assert.Contains(expectedFragment, syntax);
        }

        Assert.Equal(expectedDefinedVariables, syntaxVariables);
    }

    private static string RenderSyntax(BlockInstance block, List<string> definedVariables)
        => block.ToSyntax(new BlockSyntaxGenerationContext(definedVariables, new ConfigSettings())).ToSnippet();
}
