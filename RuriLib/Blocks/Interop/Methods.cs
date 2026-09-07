using Jering.Javascript.NodeJS;
using Jint;
using Microsoft.Scripting.Hosting;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Variables;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuriLib.Exceptions;

namespace RuriLib.Blocks.Interop;

/// <summary>
/// Blocks and helpers for interoperability with other programs.
/// </summary>
[BlockCategory("Interop", "Blocks for interoperability with other programs", "#ddadaf")]
public static class Methods
{
    /// <summary>
    /// Executes a shell command and redirects all stdout to the output variable.
    /// </summary>
    // Compatibility wrapper for existing direct C# callers; blocks use ShellCommandAsync.
    public static string ShellCommand(BotData data, string executable, string arguments)
        => ShellCommandAsync(data, executable, arguments).GetAwaiter().GetResult();

    /// <summary>
    /// Executes a shell command and redirects all stdout to the output variable.
    /// </summary>
    public static async Task<string> ShellCommandAsync(BotData data, string executable, string arguments)
        => await ShellCommandAsync(data, executable, arguments, Timeout.Infinite).ConfigureAwait(false);

    /// <summary>
    /// Executes a shell command and redirects all stdout to the output variable.
    /// </summary>
    [Block("Executes a shell command and redirects all stdout to the output variable", id = nameof(ShellCommand))]
    public static async Task<string> ShellCommandAsync(BotData data, string executable, string arguments,
        int timeoutMilliseconds = 30000)
    {
        data.Logger.LogHeader();

        // For example executable is C:\Python27\python.exe and arguments is C:\sample_script.py
        var start = new ProcessStartInfo(executable, arguments)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(start);

        if (process is null)
        {
            data.Logger.Log("The process could not be started", LogColors.PaleChestnut);
            return string.Empty;
        }

        using var timeoutCts = timeoutMilliseconds == Timeout.Infinite
            ? null
            : new CancellationTokenSource(timeoutMilliseconds);
        using var linkedCts = timeoutCts is null
            ? CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(data.CancellationToken, timeoutCts.Token);

        using var reader = process.StandardOutput;

        try
        {
            var result = await reader.ReadToEndAsync(linkedCts.Token).ConfigureAwait(false);
            await process.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);
            data.Logger.Log($"Standard Output:", LogColors.PaleChestnut);
            data.Logger.Log(result, LogColors.PaleChestnut);
            return result;
        }
        catch (OperationCanceledException) when (timeoutCts?.IsCancellationRequested == true &&
                                                 !data.CancellationToken.IsCancellationRequested)
        {
            TryKillProcess(process);
            throw new TimeoutException($"The process timed out after {timeoutMilliseconds} ms");
        }
        catch
        {
            TryKillProcess(process);
            throw;
        }
    }

    private static void TryKillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Ignore cleanup errors and preserve the original exception.
        }
    }

    /*
     * These are not blocks, but they take BotData as an input. The ScriptBlockInstance will take care
     * of writing C# code that calls these methods where necessary once it's transpiled.
     */
    /// <summary>
    /// Invokes a NodeJS script from a file or inline source.
    /// </summary>
    public static async Task<T?> InvokeNode<T>(BotData data, string scriptOrFile, object[] parameters, bool isScript = false, string? scriptHash = null)
    {
        data.Logger.LogHeader();

        T? result;

        if (isScript)
        {
            result = await InvokeFromStringOrCache<T>(data, scriptOrFile, parameters, scriptHash);
        }
        else
        {
            result = await InvokeFromFile<T>(data, scriptOrFile, parameters);
        }

        data.Logger.Log($"Executed NodeJS script with result: {result}", LogColors.PaleChestnut);
        return result;
    }

    private static async Task<T?> InvokeFromStringOrCache<T>(BotData data, string script, object[] parameters, string? scriptHash = null)
    {
        if (string.IsNullOrEmpty(scriptHash))
        {
            return await StaticNodeJSService.InvokeFromStringAsync<T>(
                script,
                scriptHash,
                null,
                parameters,
                data.CancellationToken
            ).ConfigureAwait(false);
        }

        var (isCached, cachedResult) = await StaticNodeJSService.TryInvokeFromCacheAsync<T>(
            scriptHash,
            null,
            parameters,
            data.CancellationToken
        ).ConfigureAwait(false);

        return isCached ? cachedResult : await StaticNodeJSService.InvokeFromStringAsync<T>(
            script,
            scriptHash,
            null,
            parameters,
            data.CancellationToken
        ).ConfigureAwait(false);
    }

    private static async Task<T?> InvokeFromFile<T>(BotData data, string filePath, object[] parameters)
    {
        return await StaticNodeJSService.InvokeFromFileAsync<T>(
            filePath,
            null,
            parameters,
            data.CancellationToken
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a JavaScript file using Jint.
    /// </summary>
    [Obsolete("Jint is planned for deprecation in a future release. Prefer NodeJS for new JavaScript-based configs.")]
    public static Engine InvokeJint(BotData data, Engine engine, string scriptFile)
    {
        data.Logger.LogHeader();

        var script = File.ReadAllText(scriptFile);
        var completionValue = engine.Evaluate(script);
        data.Logger.Log($"Executed Jint script with completion value: {completionValue}", LogColors.PaleChestnut);
        return engine;
    }

    /// <summary>
    /// Creates a new IronPython scope from the configured engine.
    /// </summary>
    [Obsolete("IronPython is planned for deprecation in a future release. Prefer Python for new configs.")]
    public static ScriptScope GetIronPyScope(BotData data)
    {
        data.Logger.LogHeader();

        data.Logger.Log("Getting a new IronPython scope.", LogColors.PaleChestnut);
        var engine = data.TryGetObject<ScriptEngine>("ironPyEngine");

        if (engine is null)
        {
            throw new BlockExecutionException("The IronPython engine is not initialized");
        }

        return engine.CreateScope();
    }

    /// <summary>
    /// Executes an IronPython script file in the provided scope.
    /// </summary>
    [Obsolete("IronPython is planned for deprecation in a future release. Prefer Python for new configs.")]
    public static void ExecuteIronPyScript(BotData data, ScriptScope scope, string scriptFile)
    {
        var engine = data.TryGetObject<ScriptEngine>("ironPyEngine");

        if (engine is null)
        {
            throw new BlockExecutionException("The IronPython engine is not initialized");
        }

        var code = engine.CreateScriptSourceFromFile(scriptFile);
        var result = code.Execute(scope);
        data.Logger.Log($"Executed IronPython script with result {result}", LogColors.PaleChestnut);
    }

    /// <summary>
    /// Executes a Python script with CPython via CSnakes and returns the typed outputs.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, object>> InvokePythonAsync(
        BotData data,
        string script,
        string scriptHash,
        string[] inputNames,
        object[] inputValues,
        string[] outputNames,
        VariableType[] outputTypes)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(script);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptHash);
        ArgumentNullException.ThrowIfNull(inputNames);
        ArgumentNullException.ThrowIfNull(inputValues);
        ArgumentNullException.ThrowIfNull(outputNames);
        ArgumentNullException.ThrowIfNull(outputTypes);

        data.Logger.LogHeader();

        var runtime = data.TryGetObject<PythonScriptRuntime>("pythonRuntime");

        if (runtime is null)
        {
            runtime = PythonScriptRuntime.GetShared();
            data.SetObject("pythonRuntime", runtime, false);
        }

        return await runtime
            .InvokeAsync(data, script, scriptHash, inputNames, inputValues, outputNames, outputTypes)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a boolean output value from a Python script result.
    /// </summary>
    public static bool GetPythonBoolOutput(IReadOnlyDictionary<string, object> result, string name)
        => result.TryGetValue(name, out var value) && value is bool boolean
            ? boolean
            : throw new BlockExecutionException($"Python output '{name}' is missing or is not a boolean");

    /// <summary>
    /// Gets a byte-array output value from a Python script result.
    /// </summary>
    public static byte[] GetPythonByteArrayOutput(IReadOnlyDictionary<string, object> result, string name)
        => result.TryGetValue(name, out var value) && value is byte[] bytes
            ? bytes
            : throw new BlockExecutionException($"Python output '{name}' is missing or is not a byte array");

    /// <summary>
    /// Gets a floating-point output value from a Python script result.
    /// </summary>
    public static float GetPythonFloatOutput(IReadOnlyDictionary<string, object> result, string name)
        => Convert.ToSingle(GetPythonDoubleOutput(result, name));

    /// <summary>
    /// Gets a double-precision floating-point output value from a Python script result.
    /// </summary>
    public static double GetPythonDoubleOutput(IReadOnlyDictionary<string, object> result, string name)
    {
        var value = GetRequiredPythonOutput(result, name);

        return value switch
        {
            float single => single,
            double number => number,
            _ => throw new BlockExecutionException($"Python output '{name}' is not a floating-point number")
        };
    }

    /// <summary>
    /// Gets an integer output value from a Python script result.
    /// </summary>
    public static int GetPythonIntOutput(IReadOnlyDictionary<string, object> result, string name)
        => Convert.ToInt32(GetPythonLongOutput(result, name));

    /// <summary>
    /// Gets a long integer output value from a Python script result.
    /// </summary>
    public static long GetPythonLongOutput(IReadOnlyDictionary<string, object> result, string name)
    {
        var value = GetRequiredPythonOutput(result, name);

        return value switch
        {
            int number => number,
            long number => number,
            _ => throw new BlockExecutionException($"Python output '{name}' is not an integer")
        };
    }

    /// <summary>
    /// Gets a string output value from a Python script result.
    /// </summary>
    public static string GetPythonStringOutput(IReadOnlyDictionary<string, object> result, string name)
        => result.TryGetValue(name, out var value) && value is string text
            ? text
            : throw new BlockExecutionException($"Python output '{name}' is missing or is not a string");

    /// <summary>
    /// Gets a list-of-strings output value from a Python script result.
    /// </summary>
    public static List<string> GetPythonListOfStringsOutput(IReadOnlyDictionary<string, object> result, string name)
    {
        var value = GetRequiredPythonOutput(result, name);

        return value switch
        {
            IEnumerable<string> strings => strings.ToList(),
            IEnumerable<object> objects => objects.Select(item => item as string
                ?? throw new BlockExecutionException($"Python output '{name}' contains a non-string list item"))
                .ToList(),
            _ => throw new BlockExecutionException($"Python output '{name}' is not a list of strings")
        };
    }

    /// <summary>
    /// Gets a dictionary-of-strings output value from a Python script result.
    /// </summary>
    public static Dictionary<string, string> GetPythonDictionaryOfStringsOutput(IReadOnlyDictionary<string, object> result, string name)
    {
        var value = GetRequiredPythonOutput(result, name);

        return value switch
        {
            IReadOnlyDictionary<string, string> strings => strings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            IReadOnlyDictionary<string, object> objects => objects.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value as string
                    ?? throw new BlockExecutionException($"Python output '{name}' contains a non-string dictionary value")),
            _ => throw new BlockExecutionException($"Python output '{name}' is not a dictionary of strings")
        };
    }

    private static object GetRequiredPythonOutput(IReadOnlyDictionary<string, object> result, string name)
        => result.TryGetValue(name, out var value)
            ? value
            : throw new BlockExecutionException($"Python output '{name}' is missing");
}
