using Jering.Javascript.NodeJS;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace RuriLib.Tests.Functions.Interop
{
    public class NodeJSTests
    {
        private string BuildScript(string innerScript, string[] inputs, string[] outputs)
        {
            return @$"module.exports = (callback, {MakeInputs(inputs)}) => {{
{innerScript}
var noderesult = {{
{MakeNodeObject(outputs)}
}};
callback(null, noderesult);
}}";
        }

        private string MakeNodeObject(string[] outputs)
            => string.Join("\r\n", outputs.Select(o => $"  '{o}': {o},"));

        private string MakeInputs(string[] inputs)
            => string.Join(",", inputs.Select(i => Regex.Match(i, "[A-Za-z0-9]+$")));

        [Fact]
        public void InvokeNode_IntegerSum_ReturnInteger()
        {
            var script = BuildScript(
                "var result = x + y;",
                new string[] { "x", "y" },
                new string[] { "result" });
            JsonElement result = StaticNodeJSService.InvokeFromStringAsync<dynamic>(script, args: new object[] { 3, 5 }).Result;
            Assert.Equal(8, result.GetProperty("result").GetInt32());
        }

        [Fact]
        public void InvokeNode_FloatSum_ReturnFloat()
        {
            var script = BuildScript(
                "var result = x + y;",
                new string[] { "x", "y" },
                new string[] { "result" });
            JsonElement result = StaticNodeJSService.InvokeFromStringAsync<dynamic>(script, args: new object[] { 3.5f, 5.2f }).Result;
            Assert.Equal(8.7f, result.GetProperty("result").GetSingle());
        }

        [Fact]
        public void InvokeNode_BoolAnd_ReturnBool()
        {
            var script = BuildScript(
                "var result = x && y;",
                new string[] { "x", "y" },
                new string[] { "result" });
            JsonElement result = StaticNodeJSService.InvokeFromStringAsync<dynamic>(script, args: new object[] { true, false }).Result;
            Assert.False(result.GetProperty("result").GetBoolean());
        }

        [Fact]
        public void InvokeNode_StringChain_ReturnString()
        {
            var script = BuildScript(
                "var result = x + y;",
                new string[] { "x", "y" },
                new string[] { "result" });
            JsonElement result = StaticNodeJSService.InvokeFromStringAsync<dynamic>(script, args: new object[] { "my", "string" }).Result;
            Assert.Equal("mystring", result.GetProperty("result").GetString());
        }

        [Fact]
        public void InvokeNode_OutputList_ReturnList()
        {
            var script = BuildScript(
                "var result = [ x, y ];",
                new string[] { "x", "y" },
                new string[] { "result" });
            JsonElement result = StaticNodeJSService.InvokeFromStringAsync<dynamic>(script, args: new object[] { "a", "b" }).Result;
            var outputList = result.GetProperty("result").EnumerateArray().Select(e => e.GetString()).ToList();
            Assert.Equal(2, outputList.Count);
            Assert.Equal("a", outputList[0]);
            Assert.Equal("b", outputList[1]);
        }

        [Fact]
        public void InvokeNode_InputList_ReturnString()
        {
            var inputList = new List<string> { "a", "b" };
            var script = BuildScript(
                "var result = x[0];",
                new string[] { "x" },
                new string[] { "result" });
            JsonElement result = StaticNodeJSService.InvokeFromStringAsync<dynamic>(script, args: new object[] { inputList }).Result;
            Assert.Equal("a", result.GetProperty("result").GetString());
        }

        [Fact]
        public void InvokeNode_NoInputs_ReturnString()
        {
            var script = BuildScript(
                "var result = 'hello';",
                new string[] {  },
                new string[] { "result" });
            JsonElement result = StaticNodeJSService.InvokeFromStringAsync<dynamic>(script, args: new object[] { }).Result;
            Assert.Equal("hello", result.GetProperty("result").GetString());
        }

        [Fact]
        public void InvokeNode_NoOutputs_ReturnNothing()
        {
            var script = BuildScript(
                "var result = x + y;",
                new string[] { "x", "y" },
                new string[] {  });
            JsonElement result = StaticNodeJSService.InvokeFromStringAsync<dynamic>(script, args: new object[] { 3, 5 }).Result;
            Assert.Throws<KeyNotFoundException>(() => result.GetProperty("result"));
        }
    }
}
