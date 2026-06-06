using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RuriLib.Exceptions;
using RuriLib.Helpers.CSharp;
using RuriLib.Models.Blocks;
using RuriLib.Models.Configs;
using Xunit;

namespace RuriLib.Tests.Models.Blocks;

public class LoliCodeBlockInstanceTests
{
    private readonly string _nl = Environment.NewLine;

    [Fact]
    public void ToLC_ReturnsScript()
    {
        var block = CreateBlock($"LOG data{_nl}");

        Assert.Equal($"LOG data{_nl}", block.ToLC());
    }

    [Fact]
    public void FromLC_AssignsScriptAndAdvancesLineNumber()
    {
        var block = CreateBlock();
        var script = $"LOG data{_nl}END";
        var lineNumber = 0;

        block.FromLC(ref script, ref lineNumber);

        Assert.Equal(script, block.Script);
        Assert.Equal(2, lineNumber);
    }

    [Fact]
    public void BuildScriptSnippet_UnsupportedStatement_PreservesRawLine()
        => AssertTranspilesTo("return 42;", $"return 42;{_nl}");

    [Fact]
    public void BuildScriptSnippet_TakeOne_DeclaresVariable()
        => AssertTranspilesTo("TAKEONE FROM \"MyResource\" => myString",
            $"string myString = globals.Resources[\"MyResource\"].TakeOne();{_nl}");

    [Fact]
    public void BuildScriptSnippet_TakeOne_AssignsExistingVariable()
        => AssertTranspilesTo("TAKEONE FROM \"MyResource\" => myString",
            $"myString = globals.Resources[\"MyResource\"].TakeOne();{_nl}", ["myString"]);

    [Fact]
    public void BuildScriptSnippet_Take_DeclaresVariable()
        => AssertTranspilesTo("TAKE 5 FROM \"MyResource\" => myList",
            $"List<string> myList = globals.Resources[\"MyResource\"].Take(5);{_nl}");

    [Fact]
    public void BuildScriptSnippet_CodeLabel_OutputsLabel()
        => AssertTranspilesTo("#MYLABEL", $"MYLABEL:{_nl}");

    [Fact]
    public void BuildScriptSnippet_Jump_OutputsGoto()
        => AssertTranspilesTo("JUMP #MYLABEL", $"goto MYLABEL;{_nl}");

    [Fact]
    public void BuildScriptSnippet_End_ClosesBlock()
        => AssertTranspilesTo("END", $"}}{_nl}");

    [Fact]
    public void BuildScriptSnippet_Repeat_OutputsForLoop()
    {
        var output = Transpile("REPEAT 10");

        Assert.Matches($@"^for \(var [A-Za-z0-9_]+ = 0L; [A-Za-z0-9_]+ < \(10\)\.AsLong\(\); [A-Za-z0-9_]+\+\+\){Regex.Escape(_nl)}\{{{Regex.Escape(_nl)}$",
            output);
    }

    [Fact]
    public void BuildScriptSnippet_Foreach_OutputsLoop()
        => AssertTranspilesTo("FOREACH item IN items",
            $"foreach (var item in items){_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_Log_OutputsLogObject()
        => AssertTranspilesTo("LOG myVar", $"data.Logger.LogObject(myVar);{_nl}");

    [Fact]
    public void BuildScriptSnippet_LogInterpolatedString_UsesLoliCodeInterpolation()
        => AssertTranspilesTo("LOG $\"hello <x>\"",
            $"data.Logger.LogObject($\"hello {{x}}\");{_nl}");

    [Fact]
    public void BuildScriptSnippet_CLog_OutputsColoredLogObject()
        => AssertTranspilesTo("CLOG Tomato \"hello\"",
            $"data.Logger.LogObject(\"hello\", LogColors.Tomato);{_nl}");

    [Fact]
    public void BuildScriptSnippet_CLogInterpolatedString_UsesLoliCodeInterpolation()
        => AssertTranspilesTo("CLOG YellowGreen $\"hello <x>\"",
            $"data.Logger.LogObject($\"hello {{x}}\", LogColors.YellowGreen);{_nl}");

    [Fact]
    public void BuildScriptSnippet_While_OutputsLoop()
        => AssertTranspilesTo("WHILE a < b", $"while (a < b){_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_WhileKey_OutputsConditionCheck()
        => AssertTranspilesTo("WHILE STRINGKEY @left Contains \"abc\"",
            $"while (CheckCondition(data, left.AsString(), StrComparison.Contains, \"abc\")){_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_If_OutputsCondition()
        => AssertTranspilesTo("IF a < b", $"if (a < b){_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_IfKey_OutputsConditionCheck()
        => AssertTranspilesTo("IF STRINGKEY @left Contains \"abc\"",
            $"if (CheckCondition(data, left.AsString(), StrComparison.Contains, \"abc\")){_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_InvalidIfKey_PreservesSourceLineAndColumn()
    {
        var block = CreateBlock($"LOG data{_nl}  IF STRINGKEY @left BadComparison \"abc\"");
        block.SourceLineNumber = 10;

        var ex = Assert.Throws<LoliCodeParsingException>(() => block.BuildScriptSnippet([]));

        Assert.Equal(11, ex.LineNumber);
        Assert.Equal(22, ex.ColumnNumber);
        Assert.IsType<LineParsingException>(ex.InnerException);
        Assert.Contains("Invalid StrComparison value 'BadComparison'", ex.Message);
    }

    [Fact]
    public void BuildScriptSnippet_Else_OutputsBranch()
        => AssertTranspilesTo("ELSE", $"}}{_nl}else{_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_ElseIf_OutputsBranch()
        => AssertTranspilesTo("ELSE IF a < b", $"}}{_nl}else if (a < b){_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_ElseIfKey_OutputsConditionCheck()
        => AssertTranspilesTo("ELSE IF STRINGKEY @left Contains \"abc\"",
            $"}}{_nl}else if (CheckCondition(data, left.AsString(), StrComparison.Contains, \"abc\")){_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_Try_OutputsTryBlock()
        => AssertTranspilesTo("TRY", $"try{_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_Catch_OutputsCatchBlock()
        => AssertTranspilesTo("CATCH", $"}}{_nl}catch{_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_Finally_OutputsFinallyBlock()
        => AssertTranspilesTo("FINALLY", $"}}{_nl}finally{_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_Lock_OutputsLockBlock()
        => AssertTranspilesTo("LOCK globals", $"lock(globals){_nl}{{{_nl}");

    [Fact]
    public void BuildScriptSnippet_AcquireLock_OutputsAsyncLockerCall()
        => AssertTranspilesTo("ACQUIRELOCK globals",
            $"await data.AsyncLocker.Acquire(nameof(globals), data.CancellationToken);{_nl}");

    [Fact]
    public void BuildScriptSnippet_ReleaseLock_OutputsAsyncLockerCall()
        => AssertTranspilesTo("RELEASELOCK globals",
            $"data.AsyncLocker.Release(nameof(globals));{_nl}");

    [Fact]
    public void BuildScriptSnippet_SetVar_DeclaresVariable()
        => AssertTranspilesTo("SET VAR myString \"hello\"",
            $"string myString = \"hello\";{_nl}");

    [Fact]
    public void BuildScriptSnippet_SetVar_AssignsExistingVariable()
        => AssertTranspilesTo("SET VAR myString \"hello\"",
            $"myString = \"hello\";{_nl}", ["myString"]);

    [Fact]
    public void BuildScriptSnippet_SetVarInterpolatedString_UsesLoliCodeInterpolation()
        => AssertTranspilesTo("SET VAR myString $\"hello <x>\"",
            $"string myString = $\"hello {{x}}\";{_nl}");

    [Fact]
    public void BuildScriptSnippet_SetCap_DeclaresAndMarksVariable()
        => AssertTranspilesTo("SET CAP myCapture \"hello\"",
            $"string myCapture = \"hello\";{_nl}data.MarkForCapture(nameof(myCapture));{_nl}");

    [Fact]
    public void BuildScriptSnippet_SetCap_AssignsAndMarksExistingVariable()
        => AssertTranspilesTo("SET CAP myCapture \"hello\"",
            $"myCapture = \"hello\";{_nl}data.MarkForCapture(nameof(myCapture));{_nl}", ["myCapture"]);

    [Fact]
    public void BuildScriptSnippet_SetCapInterpolatedString_UsesLoliCodeInterpolation()
        => AssertTranspilesTo("SET CAP myCapture $\"hello <x>\"",
            $"string myCapture = $\"hello {{x}}\";{_nl}data.MarkForCapture(nameof(myCapture));{_nl}");

    [Fact]
    public void BuildScriptSnippet_SetVarInStepByStepMode_TracksDebuggerVariable()
        => Assert.Equal(
            $"string myString = \"hello\";{_nl}data.SetDebuggerVariable(nameof(myString), myString);{_nl}",
            CreateBlock("SET VAR myString \"hello\"").BuildScriptSnippet([], stepByStep: true));

    [Fact]
    public void BuildScriptSnippet_SetUseProxy_LowercasesBoolean()
        => AssertTranspilesTo("SET USEPROXY TRUE", $"data.UseProxy = true;{_nl}");

    [Fact]
    public void BuildScriptSnippet_SetProxy_WithoutAuth_OutputsProxyCtor()
        => AssertTranspilesTo("SET PROXY \"127.0.0.1\" 9050 SOCKS5",
            $"data.Proxy = new Proxy(\"127.0.0.1\", 9050, ProxyType.Socks5);{_nl}");

    [Fact]
    public void BuildScriptSnippet_SetProxy_WithAuth_OutputsProxyCtor()
        => AssertTranspilesTo("SET PROXY \"127.0.0.1\" 9050 SOCKS5 \"user\" \"pass\"",
            $"data.Proxy = new Proxy(\"127.0.0.1\", 9050, ProxyType.Socks5, \"user\", \"pass\");{_nl}");

    [Fact]
    public void BuildScriptSnippet_Mark_OutputsMarkForCapture()
        => AssertTranspilesTo("MARK @myVar", $"data.MarkForCapture(nameof(myVar));{_nl}");

    [Fact]
    public void BuildScriptSnippet_Unmark_OutputsUnmarkCapture()
        => AssertTranspilesTo("UNMARK @myVar", $"data.UnmarkCapture(nameof(myVar));{_nl}");

    [Fact]
    public void ToSyntax_TakeOne_MatchesNormalizedLegacyOutput()
    {
        var block = CreateBlock("TAKEONE FROM \"MyResource\" => myString");

        Assert.Equal(
            $"string myString = globals.Resources[\"MyResource\"].TakeOne();{_nl}",
            block.ToSyntax(new BlockSyntaxGenerationContext([], new ConfigSettings())).ToSnippet());
    }

    [Fact]
    public void ToSyntax_MultilinePlainCSharp_PreservesWholeStatementBlock()
    {
        var script = $"if (a < b){_nl}{{{_nl}    return 42;{_nl}}}{_nl}";
        var block = CreateBlock(script);

        Assert.Equal(
            $"if (a < b){_nl}{{{_nl}    return 42;{_nl}}}{_nl}",
            block.ToSyntax(new BlockSyntaxGenerationContext([], new ConfigSettings())).ToSnippet());
    }

    private LoliCodeBlockInstance CreateBlock(string script = "")
        => new(new LoliCodeBlockDescriptor()) { Script = script };

    private string Transpile(string script)
        => Transpile(script, []);

    private string Transpile(string script, List<string> definedVariables)
        => CreateBlock(script).BuildScriptSnippet(definedVariables);

    private void AssertTranspilesTo(string script, string expected)
        => Assert.Equal(expected, Transpile(script));

    private void AssertTranspilesTo(string script, string expected, List<string> definedVariables)
        => Assert.Equal(expected, Transpile(script, definedVariables));
}
