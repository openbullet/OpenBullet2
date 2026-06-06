using Jering.Javascript.NodeJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Functions.Interop;

public class NodeJsTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    static NodeJsTests()
    {
        StaticNodeJSService.Configure<OutOfProcessNodeJSServiceOptions>(options =>
        {
            options.ConnectionTimeoutMS = 15000;
            options.NumConnectionRetries = 1;
        });
    }

    private static string BuildScript(string innerScript, string[] inputs, string[] outputs) => @$"module.exports = (callback, {MakeInputs(inputs)}) => {{
{innerScript}
var noderesult = {{
{MakeNodeObject(outputs)}
}};
callback(null, noderesult);
}}";

    private static string MakeNodeObject(string[] outputs)
        => string.Join(Environment.NewLine, outputs.Select(o => $"  '{o}': {o},"));

    private static string MakeInputs(string[] inputs)
        => string.Join(",", inputs.Select(i => Regex.Match(i, "[A-Za-z0-9]+$")));

    [Fact]
    public async Task InvokeNode_IntegerSum_ReturnInteger()
    {
        var script = BuildScript("var result = x + y;", ["x", "y"], ["result"]);
        var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(script, null, null, [3, 5], TestCancellationToken);
        Assert.Equal(8, result.GetProperty("result").GetInt32());
    }

    [Fact]
    public async Task InvokeNode_FloatSum_ReturnFloat()
    {
        var script = BuildScript("var result = x + y;", ["x", "y"], ["result"]);
        var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(script, null, null, [3.5f, 5.2f], TestCancellationToken);
        Assert.Equal(8.7f, result.GetProperty("result").GetSingle());
    }

    [Fact]
    public async Task InvokeNode_BoolAnd_ReturnBool()
    {
        var script = BuildScript("var result = x && y;", ["x", "y"], ["result"]);
        var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(script, null, null, [true, false], TestCancellationToken);
        Assert.False(result.GetProperty("result").GetBoolean());
    }

    [Fact]
    public async Task InvokeNode_StringChain_ReturnString()
    {
        var script = BuildScript("var result = x + y;", ["x", "y"], ["result"]);
        var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(script, null, null, ["my", "string"], TestCancellationToken);
        Assert.Equal("mystring", result.GetProperty("result").GetString());
    }

    [Fact]
    public async Task InvokeNode_OutputList_ReturnList()
    {
        var script = BuildScript("var result = [ x, y ];", ["x", "y"], ["result"]);
        var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(script, null, null, ["a", "b"], TestCancellationToken);
        var outputList = result.GetProperty("result").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Equal(2, outputList.Count);
        Assert.Equal("a", outputList[0]);
        Assert.Equal("b", outputList[1]);
    }

    [Fact]
    public async Task InvokeNode_OutputDictionary_ReturnsDictionary()
    {
        var script = BuildScript("var result = { x: 'a', y };", ["y"], ["result"]);
        var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(script, null, null, ["b"], TestCancellationToken);
        var outputDict = result.GetProperty("result").EnumerateObject().ToDictionary(e => e.Name, e => e.Value.GetString());
        Assert.Equal(2, outputDict.Count);
        Assert.Equal("a", outputDict["x"]);
        Assert.Equal("b", outputDict["y"]);
    }

    [Fact]
    public async Task InvokeNode_InputList_ReturnString()
    {
        List<string> inputList = ["a", "b"];
        var script = BuildScript("var result = x[0];", ["x"], ["result"]);
        var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(script, null, null, [inputList], TestCancellationToken);
        Assert.Equal("a", result.GetProperty("result").GetString());
    }

    [Fact]
    public async Task InvokeNode_NoInputs_ReturnString()
    {
        var script = BuildScript("var result = 'hello';", [], ["result"]);
        var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(script, null, null, [], TestCancellationToken);
        Assert.Equal("hello", result.GetProperty("result").GetString());
    }

    [Fact]
    public async Task InvokeNode_NoOutputs_ReturnNothing()
    {
        var script = BuildScript("var result = x + y;", ["x", "y"], []);
        var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(script, null, null, [3, 5], TestCancellationToken);
        Assert.Throws<KeyNotFoundException>(() => result.GetProperty("result"));
    }

    [Fact]
    public async Task InvokeNode_WithCreateRequire_ResolvesDependencyFromScriptsNodeModules()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"{nameof(NodeJsTests)}-{Guid.NewGuid():N}");
        var scriptsPath = Path.Combine(tempRoot, "Scripts");
        var dependencyPath = Path.Combine(scriptsPath, "node_modules", "ob2-test-dependency");

        Directory.CreateDirectory(dependencyPath);
        await File.WriteAllTextAsync(
            Path.Combine(dependencyPath, "index.js"),
            "module.exports = { getValue: () => 'resolved from Scripts/node_modules' };",
            TestCancellationToken);

        var virtualScriptPath = Path.Combine(scriptsPath, "__ob2_virtual__.js");
        var escapedVirtualScriptPath = JsonSerializer.Serialize(virtualScriptPath);
        var script = $$"""
            module.exports = async () => {
                const { createRequire } = require('module');
                const obRequire = createRequire({{escapedVirtualScriptPath}});
                return await (async (require) => {
                    const dependency = require('ob2-test-dependency');
                    var result = dependency.getValue();
                    var noderesult = {
                      'result': result,
                    };
                    return noderesult;
                })(obRequire);
            }
            """;

        try
        {
            var result = await StaticNodeJSService.InvokeFromStringAsync<JsonElement>(
                script,
                null,
                null,
                [],
                TestCancellationToken);

            Assert.Equal("resolved from Scripts/node_modules", result.GetProperty("result").GetString());
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
