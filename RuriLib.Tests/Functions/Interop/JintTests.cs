using Jint;
using RuriLib.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Functions.Interop
{
    public class JintTests
    {
        [Fact]
        public void InvokeJint_IntegerSum_ReturnInteger()
        {
            var engine = new Engine();
            engine.SetValue("x", 3);
            engine.SetValue("y", 5);
            engine.Execute("var result = x + y;");
            Assert.Equal(8, engine.Global.GetProperty("result").Value.AsNumber().ToInt());
        }

        [Fact]
        public void InvokeJint_FloatSum_ReturnFloat()
        {
            var engine = new Engine();
            engine.SetValue("x", 3.5f);
            engine.SetValue("y", 5.2f);
            engine.Execute("var result = x + y;");
            Assert.Equal(8.7f, engine.Global.GetProperty("result").Value.AsNumber().ToSingle());
        }

        [Fact]
        public void InvokeJint_StringChain_ReturnString()
        {
            var engine = new Engine();
            engine.SetValue("x", "my");
            engine.SetValue("y", "string");
            engine.Execute("var result = x + y;");
            Assert.Equal("mystring", engine.Global.GetProperty("result").Value.AsString());
        }

        [Fact]
        public void InvokeJint_OutputList_ReturnList()
        {
            var engine = new Engine();
            engine.SetValue("x", "a");
            engine.SetValue("y", "b");
            engine.Execute("var result = [ x, y ];");
            var outputList = engine.Global.GetProperty("result").Value.AsArray().GetEnumerator().ToEnumerable().ToList();
            Assert.Equal(2, outputList.Count);
            Assert.Equal("a", outputList[0]);
            Assert.Equal("b", outputList[1]);
        }

        [Fact]
        public void InvokeJint_InputList_ReturnString()
        {
            var engine = new Engine();
            engine.SetValue("x", new List<string> { "a", "b" });
            engine.Execute("var result = x[0];");
            Assert.Equal("a", engine.Global.GetProperty("result").Value.AsString());
        }

        [Fact]
        public void InvokeJint_NoOutputs_ReturnNothing()
        {
            var engine = new Engine();
            engine.SetValue("x", 1);
            engine.Execute("var y = x + 1;");
            Assert.Null(engine.Global.GetProperty("result").Value);
        }
    }
}
