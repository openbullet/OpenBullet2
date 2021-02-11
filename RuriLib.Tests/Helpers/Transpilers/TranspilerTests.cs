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
            var script = "#CodeLabel\r\nif(a < b) {\r\nBLOCK:ConstantString\r\n  => VAR @outputVar\r\nENDBLOCK\r\nJUMP #CodeLabel\r\n}\r\n";
            var stack = Loli2StackTranspiler.Transpile(script);
            var newScript = Stack2LoliTranspiler.Transpile(stack);

            Assert.Equal(script, newScript);
        }
    }
}
