using RuriLib.Models.Configs;

namespace RuriLib.Helpers.Transpilers
{
    public static class Loli2CSharpTranspiler
    {
        public static string Transpile(string script, ConfigSettings settings)
        {
            var stack = Loli2StackTranspiler.Transpile(script);
            return Stack2CSharpTranspiler.Transpile(stack, settings);
        }
    }
}
