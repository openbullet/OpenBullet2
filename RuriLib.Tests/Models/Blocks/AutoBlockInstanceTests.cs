using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using RuriLib.Models.Configs;
using System.Collections.Generic;
using Xunit;

namespace RuriLib.Tests.Models.Blocks
{
    public class AutoBlockInstanceTests
    {
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
            (index.FixedSetting as IntSetting).Value = 3;

            length.InputMode = SettingInputMode.Fixed;

            var expected = "DISABLED\r\nLABEL:My Label\r\n  input = @myInput\r\n  index = 3\r\n  => VAR @myOutput\r\n";
            Assert.Equal(expected, block.ToLC());
        }

        [Fact]
        public void FromLC_ParseLRBlock_BuildBlock()
        {
            var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
            var script = "DISABLED\r\nLABEL:My Label\r\n  input = @myInput\r\n  index = 3\r\n  => CAP @myOutput\r\n";
            int lineNumber = 0;
            block.FromLC(ref script, ref lineNumber);

            Assert.True(block.Disabled);
            Assert.Equal("My Label", block.Label);
            Assert.Equal("myOutput", block.OutputVariable);
            Assert.True(block.IsCapture);

            var input = block.Settings["input"];
            var index = block.Settings["index"];
            var length = block.Settings["length"];

            Assert.Equal(SettingInputMode.Variable, input.InputMode);
            Assert.Equal("myInput", input.InputVariableName);
            Assert.Equal(SettingInputMode.Fixed, index.InputMode);
            Assert.Equal(3, (index.FixedSetting as IntSetting).Value);
        }

        [Fact]
        public void ToCSharp_SyncReturnValue_OutputScript()
        {
            var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
            block.OutputVariable = "myOutput";
            var input = block.Settings["input"];
            var index = block.Settings["index"];
            var length = block.Settings["length"];

            input.InputMode = SettingInputMode.Variable;
            input.InputVariableName = "myInput";

            index.InputMode = SettingInputMode.Fixed;
            (index.FixedSetting as IntSetting).Value = 3;

            length.InputMode = SettingInputMode.Fixed;
            (length.FixedSetting as IntSetting).Value = 5;

            var declaredVariables = new List<string> { };

            var expected = "string myOutput = Substring(data, myInput.AsString(), 3, 5);\r\ndata.LogVariableAssignment(nameof(myOutput));\r\n";
            Assert.Equal(expected, block.ToCSharp(declaredVariables, new ConfigSettings()));
        }

        [Fact]
        public void ToCSharp_SyncReturnValueCapture_OutputScript()
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
            (index.FixedSetting as IntSetting).Value = 3;

            length.InputMode = SettingInputMode.Fixed;
            (length.FixedSetting as IntSetting).Value = 5;

            var declaredVariables = new List<string> { };

            var expected = "string myOutput = Substring(data, myInput.AsString(), 3, 5);\r\ndata.LogVariableAssignment(nameof(myOutput));\r\ndata.MarkForCapture(nameof(myOutput));\r\n";
            Assert.Equal(expected, block.ToCSharp(declaredVariables, new ConfigSettings()));
        }

        [Fact]
        public void ToCSharp_AsyncNoReturnValue_OutputScript()
        {
            var block = BlockFactory.GetBlock<AutoBlockInstance>("TcpConnect");
            var url = block.Settings["host"];
            var port = block.Settings["port"];
            var ssl = block.Settings["useSSL"];
            var timeout = block.Settings["timeoutMilliseconds"];

            (url.FixedSetting as StringSetting).Value = "example.com";
            (port.FixedSetting as IntSetting).Value = 80;
            (ssl.FixedSetting as BoolSetting).Value = false;
            (timeout.FixedSetting as IntSetting).Value = 1000;

            var declaredVariables = new List<string> { };

            var expected = "await TcpConnect(data, \"example.com\", 80, false, 1000).ConfigureAwait(false);\r\n";
            Assert.Equal(expected, block.ToCSharp(declaredVariables, new ConfigSettings()));
        }

        [Fact]
        public void ToCSharp_SyncReturnValueAlreadyDeclared_OutputScript()
        {
            var block = BlockFactory.GetBlock<AutoBlockInstance>("Substring");
            block.OutputVariable = "myOutput";
            var input = block.Settings["input"];
            var index = block.Settings["index"];
            var length = block.Settings["length"];

            input.InputMode = SettingInputMode.Interpolated;
            input.InterpolatedSetting = new InterpolatedStringSetting { Value = "my <interp> string" };

            index.InputMode = SettingInputMode.Fixed;
            (index.FixedSetting as IntSetting).Value = 3;

            length.InputMode = SettingInputMode.Variable;
            length.InputVariableName = "myLength";

            var declaredVariables = new List<string> { "myOutput" };

            var expected = "myOutput = Substring(data, $\"my {interp} string\", 3, myLength.AsInt());\r\ndata.LogVariableAssignment(nameof(myOutput));\r\n";
            Assert.Equal(expected, block.ToCSharp(declaredVariables, new ConfigSettings()));
        }
    }
}
