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
        ScriptEngine _engine;

        public IronPyTests()
        {
            var runtime = Python.CreateRuntime();
            _engine = runtime.GetEngine("py");
            PythonCompilerOptions pco = (PythonCompilerOptions)_engine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;
        }

        [Fact]
        public void ExecuteIronPyScript_IntegerSum_ReturnInteger()
        {
            var scope = _engine.CreateScope();
            scope.SetVariable("x", 3);
            scope.SetVariable("y", 5);
            var code = _engine.CreateScriptSourceFromString("result = x + y");
            code.Execute(scope);
            Assert.Equal(8, scope.GetVariable<int>("result"));
        }

        [Fact]
        public void ExecuteIronPyScript_FloatSum_ReturnFloat()
        {
            var scope = _engine.CreateScope();
            scope.SetVariable("x", 3.5f);
            scope.SetVariable("y", 5.2f);
            var code = _engine.CreateScriptSourceFromString("result = x + y");
            code.Execute(scope);
            Assert.Equal(8.7f, scope.GetVariable<float>("result"));
        }

        [Fact]
        public void ExecuteIronPyScript_StringChain_ReturnString()
        {
            var scope = _engine.CreateScope();
            scope.SetVariable("x", "my");
            scope.SetVariable("y", "string");
            var code = _engine.CreateScriptSourceFromString("result = x + y");
            code.Execute(scope);
            Assert.Equal("mystring", scope.GetVariable<string>("result"));
        }

        [Fact]
        public void ExecuteIronPyScript_OutputList_ReturnList()
        {
            var scope = _engine.CreateScope();
            scope.SetVariable("x", "a");
            scope.SetVariable("y", "b");
            var code = _engine.CreateScriptSourceFromString("result = [ x, y ]");
            code.Execute(scope);
            var outputList = scope.GetVariable<IList<object>>("result").Cast<string>().ToList();
            Assert.Equal(2, outputList.Count);
            Assert.Equal("a", outputList[0]);
            Assert.Equal("b", outputList[1]);
        }

        [Fact]
        public void ExecuteIronPyScript_InputList_ReturnString()
        {
            var scope = _engine.CreateScope();
            scope.SetVariable("x", new List<string> { "a", "b" });
            var code = _engine.CreateScriptSourceFromString("result = x[0]");
            code.Execute(scope);
            Assert.Equal("a", scope.GetVariable<string>("result"));
        }

        [Fact]
        public void ExecuteIronPyScript_NoOutputs_ReturnNothing()
        {
            var scope = _engine.CreateScope();
            scope.SetVariable("x", 1);
            var code = _engine.CreateScriptSourceFromString("y = x + 1");
            code.Execute(scope);
            Assert.Throws<MissingMemberException>(() => scope.GetVariable<string>("result"));
        }
    }
}
