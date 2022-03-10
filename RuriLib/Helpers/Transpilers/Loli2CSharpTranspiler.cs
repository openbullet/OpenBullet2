using RuriLib.Models.Configs;
using RuriLib.Helpers.CSharp;

namespace RuriLib.Helpers.Transpilers
{
    /// <summary>
    /// Takes care of transpiling LoliCode to C#.
    /// </summary>
    public static class Loli2CSharpTranspiler
    {
        /// <summary>
        /// Transpiles a LoliCode script to a C# script string.
        /// You can use the <see cref="ScriptBuilder"/> to compile it to an executable script.
        /// </summary>
        public static string Transpile(string script, ConfigSettings settings, bool stepByStep = false)
        {
            var stack = Loli2StackTranspiler.Transpile(script);
            return Stack2CSharpTranspiler.Transpile(stack, settings, stepByStep);
        }
    }
}
