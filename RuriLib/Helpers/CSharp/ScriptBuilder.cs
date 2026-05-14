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

namespace RuriLib.Helpers.CSharp;

/// <summary>
/// In charge of building the final executable C# script from a string of C# code.
/// </summary>
public class ScriptBuilder
{
    private readonly StringWriter preScript = new();
    private readonly StringWriter postScript = new();

    /// <summary>
    /// Builds an executable C# <see cref="Script" /> from a <paramref name="cSharpScript" /> string,
    /// some <paramref name="settings" /> and a <paramref name="pluginRepo" /> to reference the correct assemblies.
    /// </summary>
    public Script Build(string cSharpScript, ScriptSettings settings, PluginRepository pluginRepo)
    {
        ArgumentNullException.ThrowIfNull(cSharpScript);
        ArgumentNullException.ThrowIfNull(settings);

        var ruriLib = typeof(ScriptBuilder).Assembly;
        var plugins = pluginRepo?.GetPlugins() ?? [];

        var options = ScriptOptions.Default
            .WithReferences([ruriLib, .. plugins])
            .WithImports(GetImports(settings));

        // Add references from RuriLib
        var ruriLibReferences = ruriLib.GetReferencedAssemblies();
        options = options.AddReferences(AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => ruriLibReferences.Any(r => r.FullName == a.FullName)));

        // Add references from plugins
        var pluginReferences = plugins.SelectMany(p => p.GetReferencedAssemblies());
        options = options.AddReferences(AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => pluginReferences.Any(p => p.FullName == a.FullName)));

        return CSharpScript.Create(
            code: preScript + cSharpScript + postScript.ToString(),
            options: options,
            globalsType: typeof(ScriptGlobals));
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
            "System.Globalization",
            "System.Linq",
            "System.Net.Security",
            "System.Text.Json",
            "System.Text.RegularExpressions",
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
        => GetUsings().Concat(settings.CustomUsings
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(ParseUsing))
            .Distinct();

    private static string ParseUsing(string u)
    {
        // If the user typed the whole 'using MyLib.Test;' line, parse it to 'MyLib.Test'
        var trimmed = u.Trim();
        var match = Regex.Match(trimmed, "^using (.+);$");

        return match.Success ? match.Groups[1].Value : trimmed;
    }
}
