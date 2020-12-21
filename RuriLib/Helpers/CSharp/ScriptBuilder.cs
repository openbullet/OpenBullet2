using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Variables;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RuriLib.Helpers.CSharp
{
    public class ScriptBuilder
    {
        private StringWriter preScript = new StringWriter();
        private StringWriter postScript = new StringWriter();

        public Script Build(string cSharpScript, ScriptSettings settings)
        {
            var ruriLib = Assembly.GetAssembly(typeof(ScriptBuilder));
            var references = ruriLib.GetReferencedAssemblies();

            var script =
                CSharpScript.Create(
                    code: preScript.ToString() + cSharpScript + postScript.ToString(),
                    options: ScriptOptions.Default
                        .WithReferences(ruriLib)
                        .WithImports(settings.CustomUsings != null ? GetUsings().Concat(settings.CustomUsings) : GetUsings()),
                    globalsType: typeof(ScriptGlobals));

            script.Options.AddReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => references.Any(r => r.FullName == a.FullName)));

            return script;
        }

        /// <summary>
        /// Gets the basic usings that the C# script requires in order to be successfully executed.
        /// </summary>
        public static IEnumerable<string> GetUsings()
        {
            List<string> usings = new List<string>
            {
                "RuriLib.Logging",
                "RuriLib.Extensions",
                "RuriLib.Models.Bots",
                "RuriLib.Models.Conditions.Comparisons",
                "System.Collections.Generic",
                "System.Net.Security",
                "RuriLib.Models.Blocks.Custom.HttpRequest.Multipart",
                "Jering.Javascript.NodeJS",
                "Jint"
            };
            usings.AddRange(Globals.DescriptorsRepository.Descriptors.Values.Select(d => d.Category.Namespace).Distinct());
            return usings;
        }
    }
}
