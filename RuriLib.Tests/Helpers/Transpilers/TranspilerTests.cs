using System;
using RuriLib.Helpers.Transpilers;
using Xunit;

namespace RuriLib.Tests.Helpers.Transpilers;

public class TranspilerTests
{
    [Fact]
    public void Transpile_LoliStackLoliRoundTrip_SameScript()
    {
        var nl = Environment.NewLine;
        var script = $"#CodeLabel{nl}if(a < b) {{{nl}BLOCK:ConstantString{nl}  => VAR @outputVar{nl}ENDBLOCK{nl}JUMP #CodeLabel{nl}}}{nl}";
        var stack = Loli2StackTranspiler.Transpile(script);
        var newScript = Stack2LoliTranspiler.Transpile(stack);

        Assert.Equal(script, newScript);
    }
}
