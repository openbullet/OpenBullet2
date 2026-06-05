using System;
using System.Collections.Generic;
using RuriLib.Exceptions;
using RuriLib.Helpers.CSharp;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.Parse;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Configs;
using Xunit;

namespace RuriLib.Tests.Models.Blocks.Custom;

public class ParseBlockInstanceTests
{
    private readonly string _nl = Environment.NewLine;

    [Fact]
    public void ToLC_WritesParseBlockFormat()
    {
        var block = CreateBlock();
        block.Safe = true;
        block.Recursive = true;
        block.Mode = ParseMode.Regex;
        block.IsCapture = true;
        block.OutputVariable = "parsedOutput";

        var input = block.Settings["input"];
        input.InputMode = SettingInputMode.Variable;
        input.InputVariableName = "data.SOURCE";

        var pattern = block.Settings["pattern"];
        pattern.InputMode = SettingInputMode.Fixed;
        (pattern.FixedSetting as StringSetting)!.Value = "abc(.*)";

        var outputFormat = block.Settings["outputFormat"];
        outputFormat.InputMode = SettingInputMode.Fixed;
        (outputFormat.FixedSetting as StringSetting)!.Value = "$1";

        var multiLine = block.Settings["multiLine"];
        multiLine.InputMode = SettingInputMode.Fixed;
        (multiLine.FixedSetting as BoolSetting)!.Value = true;

        var expected = $"  input = @data.SOURCE{_nl}  pattern = \"abc(.*)\"{_nl}  outputFormat = \"$1\"{_nl}  multiLine = True{_nl}  SAFE{_nl}  RECURSIVE{_nl}  MODE:Regex{_nl}  => CAP @parsedOutput{_nl}";
        Assert.Equal(expected, block.ToLC());
    }

    [Fact]
    public void FromLC_ParsesParseBlockFormat()
    {
        var block = CreateBlock();
        var script = $"  input = @data.SOURCE{_nl}  leftDelim = \"hello\"{_nl}  rightDelim = \"you\"{_nl}  caseSensitive = False{_nl}  SAFE{_nl}  RECURSIVE{_nl}  MODE:LR{_nl}  => VAR @parsedOutput";
        var lineNumber = 0;

        block.FromLC(ref script, ref lineNumber);

        Assert.True(block.Safe);
        Assert.True(block.Recursive);
        Assert.False(block.IsCapture);
        Assert.Equal(ParseMode.LR, block.Mode);
        Assert.Equal("parsedOutput", block.OutputVariable);

        var input = block.Settings["input"];
        var leftDelim = block.Settings["leftDelim"];
        var rightDelim = block.Settings["rightDelim"];
        var caseSensitive = block.Settings["caseSensitive"];

        Assert.Equal(SettingInputMode.Variable, input.InputMode);
        Assert.Equal("data.SOURCE", input.InputVariableName);
        Assert.Equal("hello", (leftDelim.FixedSetting as StringSetting)!.Value);
        Assert.Equal("you", (rightDelim.FixedSetting as StringSetting)!.Value);
        Assert.False((caseSensitive.FixedSetting as BoolSetting)!.Value);
        Assert.Equal(8, lineNumber);
    }

    [Fact]
    public void FromLC_InvalidSetting_PreservesLineParsingDetails()
    {
        var block = CreateBlock();
        var script = $"  SAFE{_nl}  input{_nl}";
        var lineNumber = 0;

        var ex = Assert.Throws<LoliCodeParsingException>(() => block.FromLC(ref script, ref lineNumber));

        Assert.Equal(2, ex.LineNumber);
        Assert.Equal(6, ex.ColumnNumber);
        Assert.IsType<LineParsingException>(ex.InnerException);
        Assert.Contains("Expected '=' after setting name 'input'", ex.Message);
    }

    [Fact]
    public void ToSyntax_SafeRegexCapture_WritesExpectedCode()
    {
        var block = CreateBlock();
        block.Safe = true;
        block.Mode = ParseMode.Regex;
        block.IsCapture = true;
        block.OutputVariable = "parsedOutput";

        var input = block.Settings["input"];
        input.InputMode = SettingInputMode.Variable;
        input.InputVariableName = "data.SOURCE";

        var pattern = block.Settings["pattern"];
        pattern.InputMode = SettingInputMode.Fixed;
        (pattern.FixedSetting as StringSetting)!.Value = "abc(.*)";

        var outputFormat = block.Settings["outputFormat"];
        outputFormat.InputMode = SettingInputMode.Fixed;
        (outputFormat.FixedSetting as StringSetting)!.Value = "$1";

        var multiLine = block.Settings["multiLine"];
        multiLine.InputMode = SettingInputMode.Fixed;
        (multiLine.FixedSetting as BoolSetting)!.Value = true;

        var expected = $"string parsedOutput = string.Empty;{_nl}try {{{_nl}parsedOutput = MatchRegexGroups(data, data.SOURCE.AsString(), \"abc(.*)\", \"$1\", true, null, null, false);{_nl}data.LogVariableAssignment(nameof(parsedOutput));{_nl}data.MarkForCapture(nameof(parsedOutput));{_nl}}} catch (Exception safeException) {{{_nl}data.ERROR = safeException.PrettyPrint();{_nl}data.Logger.Log($\"[SAFE MODE] Exception caught and saved to data.ERROR: {{data.ERROR}}\", LogColors.Tomato); }}{_nl}";
        Assert.Equal(NormalizeSnippet(expected), RenderSyntax(block, []));
    }

    [Fact]
    public void ToSyntax_TracksExpectedPatternsAndVariableFlow()
    {
        AssertSyntax(CreateLrBlock(), [],
            ["parsedOutput"],
            "string parsedOutput = ParseBetweenStrings(data, ObjectExtensions.DynamicAsString(globals.source), \"left\", \"right\", true, \"pre-\", \"-post\", true);",
            "data.LogVariableAssignment(nameof(parsedOutput));");

        AssertSyntax(CreateCssBlock(alreadyDeclared: true), ["parsedOutput"],
            ["parsedOutput"],
            "parsedOutput = QueryCssSelector(data, data.SOURCE.AsString(), \".item\", \"href\",",
            "ObjectExtensions.DynamicAsString(globals.prefix)",
            "data.LogVariableAssignment(nameof(parsedOutput));");

        AssertSyntax(CreateXPathBlock(outputVariable: "globals.foundValue"), [],
            [],
            "globals.foundValue = QueryXPath(data, ObjectExtensions.DynamicAsString(input.html), \"//a\", \"title\",",
            "data.LogVariableAssignment(nameof(globals.foundValue));");

        AssertSyntax(CreateJsonBlock(safe: true), [],
            ["parsedJson"],
            "List<string> parsedJson = new List<string>();",
            "try",
            "parsedJson = QueryJsonTokenRecursive(data, ObjectExtensions.DynamicAsString(globals.payload), \"$.items[*].name\", \"[\", \"]\", false);",
            "catch (Exception safeException)");

        AssertSyntax(CreateRegexBlock(safe: true, isCapture: true), [],
            ["parsedOutput"],
            "data.MarkForCapture(nameof(parsedOutput));",
            "catch (Exception safeException)");
    }

    private static ParseBlockInstance CreateBlock()
        => new(new ParseBlockDescriptor());

    private static ParseBlockInstance CreateLrBlock()
    {
        var block = CreateBlock();
        block.Mode = ParseMode.LR;
        block.OutputVariable = "parsedOutput";

        block.Settings["input"].InputMode = SettingInputMode.Variable;
        block.Settings["input"].InputVariableName = "globals.source";
        (block.Settings["leftDelim"].FixedSetting as StringSetting)!.Value = "left";
        (block.Settings["rightDelim"].FixedSetting as StringSetting)!.Value = "right";
        (block.Settings["caseSensitive"].FixedSetting as BoolSetting)!.Value = true;
        (block.Settings["prefix"].FixedSetting as StringSetting)!.Value = "pre-";
        (block.Settings["suffix"].FixedSetting as StringSetting)!.Value = "-post";
        (block.Settings["urlEncodeOutput"].FixedSetting as BoolSetting)!.Value = true;

        return block;
    }

    private static ParseBlockInstance CreateCssBlock(bool alreadyDeclared = false)
    {
        var block = CreateBlock();
        block.Mode = ParseMode.CSS;
        block.OutputVariable = "parsedOutput";

        block.Settings["input"].InputMode = SettingInputMode.Variable;
        block.Settings["input"].InputVariableName = "data.SOURCE";
        (block.Settings["cssSelector"].FixedSetting as StringSetting)!.Value = ".item";
        (block.Settings["attributeName"].FixedSetting as StringSetting)!.Value = "href";

        if (alreadyDeclared)
        {
            block.Settings["prefix"].InputMode = SettingInputMode.Variable;
            block.Settings["prefix"].InputVariableName = "globals.prefix";
        }

        return block;
    }

    private static ParseBlockInstance CreateXPathBlock(string outputVariable)
    {
        var block = CreateBlock();
        block.Mode = ParseMode.XPath;
        block.OutputVariable = outputVariable;

        block.Settings["input"].InputMode = SettingInputMode.Variable;
        block.Settings["input"].InputVariableName = "input.html";
        (block.Settings["xPath"].FixedSetting as StringSetting)!.Value = "//a";
        (block.Settings["attributeName"].FixedSetting as StringSetting)!.Value = "title";

        return block;
    }

    private static ParseBlockInstance CreateJsonBlock(bool safe = false)
    {
        var block = CreateBlock();
        block.Safe = safe;
        block.Recursive = true;
        block.Mode = ParseMode.Json;
        block.OutputVariable = "parsedJson";

        block.Settings["input"].InputMode = SettingInputMode.Variable;
        block.Settings["input"].InputVariableName = "globals.payload";
        (block.Settings["jToken"].FixedSetting as StringSetting)!.Value = "$.items[*].name";
        (block.Settings["prefix"].FixedSetting as StringSetting)!.Value = "[";
        (block.Settings["suffix"].FixedSetting as StringSetting)!.Value = "]";

        return block;
    }

    private static ParseBlockInstance CreateRegexBlock(bool safe = false, bool isCapture = false)
    {
        var block = CreateBlock();
        block.Safe = safe;
        block.IsCapture = isCapture;
        block.Mode = ParseMode.Regex;
        block.OutputVariable = "parsedOutput";

        block.Settings["input"].InputMode = SettingInputMode.Variable;
        block.Settings["input"].InputVariableName = "data.SOURCE";
        (block.Settings["pattern"].FixedSetting as StringSetting)!.Value = "abc(.*)";
        (block.Settings["outputFormat"].FixedSetting as StringSetting)!.Value = "$1";
        (block.Settings["multiLine"].FixedSetting as BoolSetting)!.Value = true;

        return block;
    }

    private static void AssertSyntax(
        ParseBlockInstance block,
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

    private static string RenderSyntax(ParseBlockInstance block, List<string> definedVariables)
        => block.ToSyntax(new BlockSyntaxGenerationContext(definedVariables, new ConfigSettings())).ToSnippet();

    private static string NormalizeSnippet(string snippet)
        => StatementSyntaxParser.ParseStatements(snippet).ToSnippet();
}
