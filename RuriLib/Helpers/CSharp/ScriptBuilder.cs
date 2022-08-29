using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs.Settings;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RuriLib.Helpers.CSharp
{
    /// <summary>
    /// In charge of building the final executable C# script from a string of C# code.
    /// </summary>
    public class ScriptBuilder
    {
        private readonly StringWriter preScript = new();
        private readonly StringWriter postScript = new();

        /// <summary>
        /// Builds an executable C# <see cref="Script"/> from a <paramref name="cSharpScript"/> string,
        /// some <paramref name="settings"/> and a <paramref name="pluginRepo"/> to reference the correct assemblies.
        /// </summary>
        public Script Build(string cSharpScript, ScriptSettings settings, PluginRepository pluginRepo)
        {
            var ruriLib = Assembly.GetAssembly(typeof(ScriptBuilder));
            var plugins = pluginRepo != null ? pluginRepo.GetPlugins() : Array.Empty<Assembly>();
            
            var script =
                CSharpScript.Create(
                    code: preScript.ToString() + cSharpScript + postScript.ToString(),
                    options: ScriptOptions.Default
                        .WithReferences(new Assembly[] { ruriLib }.Concat(plugins))
                        .WithImports(GetImports(settings)),
                    globalsType: typeof(ScriptGlobals));

            // Add references from RuriLib
            var ruriLibReferences = ruriLib.GetReferencedAssemblies();
            script.Options.AddReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => ruriLibReferences.Any(r => r.FullName == a.FullName)));

            // Add references from plugins
            var pluginReferences = plugins.SelectMany(p => p.GetReferencedAssemblies());
            script.Options.AddReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => pluginReferences.Any(p => p.FullName == a.FullName)));

            return script;
        }

        /// <summary>
        /// Gets the basic usings that the C# script requires in order to be successfully executed.
        /// </summary>
        public static IEnumerable<string> GetUsings()
        {
            var usings = new List<string>
            {
                "RuriLib.Helpers",
                "RuriLib.Logging",
                "RuriLib.Extensions",
                "RuriLib.Models.Bots",
                "RuriLib.Models.Proxies",
                "RuriLib.Models.Conditions.Comparisons",
                "System.Collections.Generic",
                "System.Linq",
                "System.Net.Security",
                "RuriLib.Models.Blocks.Custom.HttpRequest.Multipart",
                "RuriLib.Functions.Http.Options",
                "Jering.Javascript.NodeJS",
                "Jint",
                "System.Threading",
                "System.Threading.Tasks",
                "System"
            };
            usings.AddRange(Globals.DescriptorsRepository.Descriptors.Values.Select(d => d.Category.Namespace).Distinct());
            return usings;
        }

        private static IEnumerable<string> GetImports(ScriptSettings settings)
            => settings.CustomUsings == null
                ? GetUsings()
                : GetUsings().Concat(settings.CustomUsings
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(ParseUsing))
                    .Distinct();

        private static string ParseUsing(string u)
        {
            // If the user typed the whole 'using MyLib.Test;' line, parse it to 'MyLib.Test'
            u = u.Trim();

            if (u.StartsWith("using"))
            {
                try
                {
                    u = Regex.Match(u, "^using (.+);$").Groups[1].Value;
                }
                catch
                {
                    
                }
            }

            return u;
        }
    }
}
