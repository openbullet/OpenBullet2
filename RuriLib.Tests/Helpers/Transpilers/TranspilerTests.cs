using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using Xunit;

namespace RuriLib.Tests.Helpers.Transpilers
{
    public class TranspilerTests
    {
        [Fact]
        public void Transpile_LoliStackLoliRoundTrip_SameScript()
        {
            var script = "#CodeLabel\r\nif(a < b) {\r\nBLOCK:ParseBetweenStrings\r\n  => VAR outputVar\r\nENDBLOCK\r\nJUMP #CodeLabel\r\n";
            var stack = Loli2StackTranspiler.Transpile(script);
            var newScript = Stack2LoliTranspiler.Transpile(stack);

            Assert.Equal(script + "\r\n", newScript);
        }

        [Fact]
        public void Transpile_Loli2CSharp_OutputScript()
        {
            var script = "#CodeLabel\r\nif(a < b) {\r\nBLOCK:ParseBetweenStrings\r\n  => VAR outputVar\r\nENDBLOCK\r\nJUMP #CodeLabel\r\n";
            var sharpScript = "CodeLabel:\r\nif(a < b) {\r\nvar outputVar = ParseBetweenStrings(data, \"\", \"\", \"\", true);\r\ngoto CodeLabel;\r\n";
            var stack = Loli2StackTranspiler.Transpile(script);
            var newScript = Stack2CSharpTranspiler.Transpile(stack, new ConfigSettings());
            
            Assert.Equal(sharpScript + "\r\n", newScript);
        }
    }
}
