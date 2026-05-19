using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RuriLib.Exceptions;
using RuriLib.Helpers.CSharp;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.Script;
using RuriLib.Models.Configs;
using RuriLib.Models.Variables;
using Xunit;

namespace RuriLib.Tests.Models.Blocks.Custom;

public class ScriptBlockInstanceTests
{
    private readonly string _nl = Environment.NewLine;

    [Fact]
    public void ToLC_WritesScriptBlockFormat()
    {
        var block = CreateBlock();

        var expected = $"INTERPRETER:Jint{_nl}INPUT x,y{_nl}BEGIN SCRIPT{_nl}var result = x + y;{_nl}END SCRIPT{_nl}OUTPUT Int @result{_nl}";
        Assert.Equal(expected, block.ToLC());
    }

    [Fact]
    public void ToLC_Python_WritesPythonVersion()
    {
        var block = CreateBlock();
        block.Interpreter = Interpreter.Python;
        block.PythonVersion = "3.11";

        var expected = $"INTERPRETER:Python{_nl}PYTHONVERSION:3.11{_nl}INPUT x,y{_nl}BEGIN SCRIPT{_nl}var result = x + y;{_nl}END SCRIPT{_nl}OUTPUT Int @result{_nl}";
        Assert.Equal(expected, block.ToLC());
    }

    [Fact]
    public void FromLC_ParsesScriptBlockFormat()
    {
        var block = CreateBlock();
        var script = $"INTERPRETER:NodeJS{_nl}INPUT input.DATA, x{_nl}BEGIN SCRIPT{_nl}var result = x + 1;{_nl}END SCRIPT{_nl}OUTPUT String @result";
        var lineNumber = 0;

        block.FromLC(ref script, ref lineNumber);

        Assert.Equal(Interpreter.NodeJS, block.Interpreter);
        Assert.Equal("input.DATA, x", block.InputVariables);
        Assert.Equal("var result = x + 1;", block.Script);
        Assert.Single(block.OutputVariables);
        Assert.Equal(VariableType.String, block.OutputVariables[0].Type);
        Assert.Equal("result", block.OutputVariables[0].Name);
        Assert.Equal(6, lineNumber);
    }

    [Fact]
    public void FromLC_Python_ParsesPythonVersion()
    {
        var block = CreateBlock();
        var script = $"INTERPRETER:Python{_nl}PYTHONVERSION:3.11{_nl}INPUT input.DATA{_nl}BEGIN SCRIPT{_nl}result = DATA{_nl}END SCRIPT{_nl}OUTPUT String @result";
        var lineNumber = 0;

        block.FromLC(ref script, ref lineNumber);

        Assert.Equal(Interpreter.Python, block.Interpreter);
        Assert.Equal("3.11", block.PythonVersion);
        Assert.Equal("input.DATA", block.InputVariables);
        Assert.Equal("result = DATA", block.Script);
        Assert.Equal(7, lineNumber);
    }

    [Fact]
    public void FromLC_MissingEndScript_Throws()
    {
        var block = CreateBlock();
        var script = $"INTERPRETER:Jint{_nl}INPUT x,y{_nl}BEGIN SCRIPT{_nl}var result = x + y;";
        var lineNumber = 0;

        Assert.Throws<LoliCodeParsingException>(() => block.FromLC(ref script, ref lineNumber));
    }

    [Fact]
    public void ToSyntax_NodeJs_DeclaresOutputsAndSanitizesInputs()
    {
        var block = CreateBlock();
        block.Interpreter = Interpreter.NodeJS;
        block.InputVariables = "input.DATA, x";
        block.Script = "var result = DATA + x;";
        block.OutputVariables =
        [
            new OutputVariable { Name = "result", Type = VariableType.String }
        ];

        var definedVariables = new List<string>();
        var output = RenderSyntax(block, definedVariables);

        Assert.Contains("module.exports = async (DATA,x) => {", output);
        Assert.Contains("const { createRequire } = require('module');", output);
        Assert.Contains("__ob2_virtual__.js", output);
        Assert.Contains("new object[] { input.DATA, x }", output);
        Assert.Contains("string result = ", output);
        Assert.Contains("result = tmp_", output);
        Assert.Contains("data.LogVariableAssignment(nameof(result));", output);
        Assert.Contains("result", definedVariables);
    }

    [Fact]
    public void ToSyntax_NodeJs_ReusesExistingOutputVariable()
    {
        var block = CreateBlock();
        block.Interpreter = Interpreter.NodeJS;

        var output = RenderSyntax(block, ["result"]);

        Assert.DoesNotContain("string result =", output);
        Assert.Contains("result = tmp_", output);
    }

    [Fact]
    public void ToSyntax_NodeJs_UsesStableGeneratedShape()
    {
        var block = CreateBlock();
        block.Interpreter = Interpreter.NodeJS;
        block.InputVariables = "input.DATA, x";
        block.Script = "var result = DATA + x;";
        block.OutputVariables =
        [
            new OutputVariable { Name = "result", Type = VariableType.String }
        ];

        var syntax = NormalizeTempNames(
            RenderSyntax(block, []));

        Assert.Contains("var tmp_TEMP = await InvokeNode<dynamic>(data,", syntax);
        Assert.Contains("createRequire", syntax);
        Assert.Contains("__ob2_virtual__.js", syntax);
        Assert.Contains("new object[] { input.DATA, x }", syntax);
        Assert.Contains("string result = tmp_TEMP.GetProperty(\"result\").ToString();", syntax);
    }

    [Fact]
    public void ToSyntax_TracksExpectedInterpreterShapesAndVariableFlow()
    {
        AssertSyntax(CreateNodeJsBlock(), [],
            ["result"],
            "InvokeNode<dynamic>(data,",
            "new object[] { input.DATA, x }",
            "string result = tmp_");

        AssertSyntax(CreateNodeJsBlock(), ["result"],
            ["result"],
            "result = tmp_",
            "data.LogVariableAssignment(nameof(result));");

        AssertSyntax(CreateJintBlock(), [],
            ["count"],
            "new Engine()",
            "SetValue(nameof(globals.source), globals.source);",
            ".SetValue(nameof(y), y);",
            "InvokeJint(data, tmp_",
            "int count = ");

        AssertSyntax(CreateIronPythonBlock(), [],
            ["message"],
            "GetIronPyScope(data);",
            "SetVariable(nameof(input.NAME), input.NAME);",
            "ExecuteIronPyScript(data,",
            "string message = ");

        AssertSyntax(CreatePythonBlock(), [],
            ["result"],
            "InvokePython(data,",
            "\"result = DATA + x\"",
            "new string[] { \"DATA\", \"x\" }",
            "new object[] { input.DATA, x }",
            "new string[] { \"result\" }",
            "\"3.12\"",
            "string result = tmp_");
    }

    private static ScriptBlockInstance CreateBlock()
        => new(new ScriptBlockDescriptor());

    private static ScriptBlockInstance CreateNodeJsBlock()
        => new(new ScriptBlockDescriptor())
        {
            Interpreter = Interpreter.NodeJS,
            InputVariables = "input.DATA, x",
            Script = "var result = DATA + x;",
            OutputVariables =
            [
                new OutputVariable { Name = "result", Type = VariableType.String }
            ]
        };

    private static ScriptBlockInstance CreateJintBlock()
        => new(new ScriptBlockDescriptor())
        {
            Interpreter = Interpreter.Jint,
            InputVariables = "globals.source, y",
            Script = "var count = source.length + y;",
            OutputVariables =
            [
                new OutputVariable { Name = "count", Type = VariableType.Int }
            ]
        };

    private static ScriptBlockInstance CreateIronPythonBlock()
        => new(new ScriptBlockDescriptor())
        {
            Interpreter = Interpreter.IronPython,
            InputVariables = "input.NAME",
            Script = "message = NAME + '_done'",
            OutputVariables =
            [
                new OutputVariable { Name = "message", Type = VariableType.String }
            ]
        };

    private static ScriptBlockInstance CreatePythonBlock()
        => new(new ScriptBlockDescriptor())
        {
            Interpreter = Interpreter.Python,
            PythonVersion = "3.12",
            InputVariables = "input.DATA, x",
            Script = "result = DATA + x",
            OutputVariables =
            [
                new OutputVariable { Name = "result", Type = VariableType.String }
            ]
        };

    private static void AssertSyntax(
        ScriptBlockInstance block,
        List<string> definedVariables,
        List<string> expectedDefinedVariables,
        params string[] expectedFragments)
    {
        var syntaxVariables = new List<string>(definedVariables);
        var syntax = NormalizeTempNames(RenderSyntax(block, syntaxVariables));

        foreach (var expectedFragment in expectedFragments)
        {
            Assert.Contains(expectedFragment, syntax);
        }

        Assert.Equal(expectedDefinedVariables, syntaxVariables);
    }

    private static string RenderSyntax(ScriptBlockInstance block, List<string> definedVariables)
        => block.ToSyntax(new BlockSyntaxGenerationContext(definedVariables, new ConfigSettings())).ToSnippet();

    private static string NormalizeTempNames(string input)
        => Regex.Replace(
            Regex.Replace(input.Replace("\\r\\n", "\\n"), "\"[a-f0-9]{32}\"", "\"HASH\""),
            @"tmp_[A-Za-z0-9_]+",
            "tmp_TEMP");
}
