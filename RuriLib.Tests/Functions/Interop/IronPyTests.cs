using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RuriLib.Tests.Functions.Interop
{
    public class IronPyTests
    {
        private readonly ScriptEngine engine;

        public IronPyTests()
        {
            var runtime = Python.CreateRuntime();
            engine = runtime.GetEngine("py");
            var pco = (PythonCompilerOptions)engine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;
        }

        [Fact]
        public void ExecuteIronPyScript_IntegerSum_ReturnInteger()
        {
            var scope = engine.CreateScope();
            scope.SetVariable("x", 3);
            scope.SetVariable("y", 5);
            var code = engine.CreateScriptSourceFromString("result = x + y");
            code.Execute(scope);
            Assert.Equal(8, scope.GetVariable<int>("result"));
        }

        [Fact]
        public void ExecuteIronPyScript_FloatSum_ReturnFloat()
        {
            var scope = engine.CreateScope();
            scope.SetVariable("x", 3.5f);
            scope.SetVariable("y", 5.2f);
            var code = engine.CreateScriptSourceFromString("result = x + y");
            code.Execute(scope);
            Assert.Equal(8.7f, scope.GetVariable<float>("result"));
        }

        [Fact]
        public void ExecuteIronPyScript_StringChain_ReturnString()
        {
            var scope = engine.CreateScope();
            scope.SetVariable("x", "my");
            scope.SetVariable("y", "string");
            var code = engine.CreateScriptSourceFromString("result = x + y");
            code.Execute(scope);
            Assert.Equal("mystring", scope.GetVariable<string>("result"));
        }

        [Fact]
        public void ExecuteIronPyScript_OutputList_ReturnList()
        {
            var scope = engine.CreateScope();
            scope.SetVariable("x", "a");
            scope.SetVariable("y", "b");
            var code = engine.CreateScriptSourceFromString("result = [ x, y ]");
            code.Execute(scope);
            var outputList = scope.GetVariable<IList<object>>("result").Cast<string>().ToList();
            Assert.Equal(2, outputList.Count);
            Assert.Equal("a", outputList[0]);
            Assert.Equal("b", outputList[1]);
        }

        [Fact]
        public void ExecuteIronPyScript_InputList_ReturnString()
        {
            var scope = engine.CreateScope();
            scope.SetVariable("x", new List<string> { "a", "b" });
            var code = engine.CreateScriptSourceFromString("result = x[0]");
            code.Execute(scope);
            Assert.Equal("a", scope.GetVariable<string>("result"));
        }

        [Fact]
        public void ExecuteIronPyScript_NoOutputs_ReturnNothing()
        {
            var scope = engine.CreateScope();
            scope.SetVariable("x", 1);
            var code = engine.CreateScriptSourceFromString("y = x + 1");
            code.Execute(scope);
            Assert.Throws<MissingMemberException>(() => scope.GetVariable<string>("result"));
        }
    }
}
