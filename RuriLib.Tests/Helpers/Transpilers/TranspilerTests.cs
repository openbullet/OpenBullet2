using System;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Blocks;
using RuriLib.Models.Configs;
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

    [Fact]
    public void Transpile_LoliAliasStackLoliRoundTrip_UsesCanonicalBlockId()
    {
        var nl = Environment.NewLine;
        var script = $"BLOCK:LookupDns{nl}  => VAR @dnsAnswers{nl}ENDBLOCK{nl}";

        var stack = Loli2StackTranspiler.Transpile(script);
        var block = Assert.IsType<AutoBlockInstance>(Assert.Single(stack));
        var newScript = Stack2LoliTranspiler.Transpile(stack);

        Assert.Equal("DnsLookup", block.Id);
        Assert.Equal($"BLOCK:DnsLookup{nl}  => VAR @dnsAnswers{nl}ENDBLOCK{nl}", newScript);
    }

    [Fact]
    public void Transpile_LoliAliasesToCSharp_UseCanonicalBlock()
    {
        var nl = Environment.NewLine;
        var canonicalScript = $"BLOCK:DnsLookup{nl}  => VAR @dnsAnswers{nl}ENDBLOCK{nl}";
        var aliasScript = $"BLOCK:LookupDnsAsync{nl}  => VAR @dnsAnswers{nl}ENDBLOCK{nl}";

        var canonical = NormalizeLineEndings(
            Loli2CSharpTranspiler.Transpile(canonicalScript, new ConfigSettings()));
        var aliased = NormalizeLineEndings(
            Loli2CSharpTranspiler.Transpile(aliasScript, new ConfigSettings()));

        Assert.Equal(canonical, aliased);
    }

    [Fact]
    public void Transpile_PuppeteerAliasStackLoliRoundTrip_UsesGenericBrowserBlock()
    {
        var nl = Environment.NewLine;
        var script = $"BLOCK:PuppeteerReload{nl}ENDBLOCK{nl}";

        var stack = Loli2StackTranspiler.Transpile(script);
        var block = Assert.IsType<AutoBlockInstance>(Assert.Single(stack));
        var newScript = Stack2LoliTranspiler.Transpile(stack);

        Assert.Equal("BrowserReload", block.Id);
        Assert.Equal($"BLOCK:BrowserReload{nl}ENDBLOCK{nl}", newScript);
    }

    private static string NormalizeLineEndings(string value) => value.Replace("\r\n", "\n");
}
