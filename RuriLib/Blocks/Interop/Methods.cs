using Jering.Javascript.NodeJS;
using Jint;
using Microsoft.Scripting.Hosting;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Interop
{
    [BlockCategory("Interop", "Blocks for interoperability with other programs", "#ddadaf")]
    public static class Methods
    {
        [Block("Executes a shell command and redirects all stdout to the output variable")]
        public static string ShellCommand(BotData data, string executable, string arguments)
        {
            // For example executable is C:\Python27\python.exe and arguments is C:\sample_script.py
            var start = new ProcessStartInfo(executable, arguments)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            data.Logger.LogHeader();

            using var process = Process.Start(start);
            using var reader = process.StandardOutput;

            var result = reader.ReadToEnd();
            data.Logger.Log($"Standard Output:", LogColors.PaleChestnut);
            data.Logger.Log(result, LogColors.PaleChestnut);
            return result;
        }

        /*
         * These are not blocks, but they take BotData as an input. The ScriptBlockInstance will take care
         * of writing C# code that calls these methods where necessary once it's transpiled.
         */

        public static async Task<T> InvokeNode<T>(BotData data, string scriptFile, object[] parameters)
        {
            data.Logger.LogHeader();
            var result = await StaticNodeJSService.InvokeFromFileAsync<T>(scriptFile, null, parameters, data.CancellationToken).ConfigureAwait(false);
            data.Logger.Log($"Executed NodeJS script with result: {result}", LogColors.PaleChestnut);
            return result;
        }

        public static Engine InvokeJint(BotData data, Engine engine, string scriptFile)
        {
            data.Logger.LogHeader();
            var script = File.ReadAllText(scriptFile);
            engine.Execute(script);
            data.Logger.Log($"Executed Jint script with completion value: {engine.GetCompletionValue()}", LogColors.PaleChestnut);
            return engine;
        }

        public static ScriptScope GetIronPyScope(BotData data)
        {
            data.Logger.LogHeader();
            data.Logger.Log($"Getting a new IronPython scope.", LogColors.PaleChestnut);
            var engine = data.TryGetObject<ScriptEngine>("ironPyEngine");
            return engine.CreateScope();
        }

        public static void ExecuteIronPyScript(BotData data, ScriptScope scope, string scriptFile)
        {
            var engine = data.TryGetObject<ScriptEngine>("ironPyEngine");
            var code = engine.CreateScriptSourceFromFile(scriptFile);
            var result = code.Execute(scope);
            data.Logger.Log($"Executed IronPython script with result {result}", LogColors.PaleChestnut);
        }
    }
}
