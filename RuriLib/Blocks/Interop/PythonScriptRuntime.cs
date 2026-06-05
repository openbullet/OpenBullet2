using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using Microsoft.Extensions.DependencyInjection;
using RuriLib.Exceptions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Interop;

internal sealed class PythonScriptRuntime : IDisposable
{
    private const string DefaultRedistributablePythonVersion = "3.14";
    private const string BridgeModuleFileName = "ob2_python_bridge.py";
    private const string BridgeModuleResourceName = "RuriLib.Blocks.Interop.ob2_python_bridge.py";
    private static readonly Lazy<PythonScriptRuntime> sharedRuntime = new(() => new PythonScriptRuntime("Scripts"));
    private readonly SemaphoreSlim initializationSemaphore = new(1, 1);
    private readonly string scriptsPath;

    private ServiceProvider? serviceProvider;
    private IPythonEnvironment? environment;
    private string? actualPythonVersion;

    public PythonScriptRuntime(string scriptsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptsPath);
        this.scriptsPath = Path.GetFullPath(scriptsPath);
    }

    public static PythonScriptRuntime GetShared() => sharedRuntime.Value;

    public async Task<IReadOnlyDictionary<string, object>> InvokeAsync(
        BotData data,
        string script,
        string scriptHash,
        string[] inputNames,
        object[] inputValues,
        string[] outputNames,
        VariableType[] outputTypes)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(script);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptHash);
        ArgumentNullException.ThrowIfNull(inputNames);
        ArgumentNullException.ThrowIfNull(inputValues);
        ArgumentNullException.ThrowIfNull(outputNames);
        ArgumentNullException.ThrowIfNull(outputTypes);

        await InitializeIfNeededAsync(data.Logger, data.CancellationToken).ConfigureAwait(false);
        data.CancellationToken.ThrowIfCancellationRequested();

        var invocationId = Guid.NewGuid().ToString("N");
        var inputs = BuildInputDictionary(inputNames, inputValues);

        try
        {
            var result = await environment!
                .Ob2PythonBridge()
                .Run(
                    invocationId,
                    scriptHash,
                    script,
                    inputs,
                    outputNames,
                    data.CancellationToken)
                .ConfigureAwait(false);

            LogCapturedOutput(data.Logger, invocationId);
            var convertedResult = ConvertOutputs(result, outputNames, outputTypes);
            data.Logger.Log($"Executed Python script with result: {FormatResultForLog(convertedResult)}", LogColors.PaleChestnut);
            return convertedResult;
        }
        catch (OperationCanceledException)
        {
            LogCapturedOutput(data.Logger, invocationId);
            throw;
        }
        catch (Exception ex)
        {
            LogCapturedOutput(data.Logger, invocationId);
            data.Logger.Log($"Python script failed: {ex.Message}", LogColors.Tomato);
            throw;
        }
        finally
        {
            foreach (var value in inputs.Values)
            {
                value.Dispose();
            }
        }
    }

    private async Task InitializeIfNeededAsync(IBotLogger logger, CancellationToken cancellationToken)
    {
        var expectedPythonVersion = ResolveExpectedPythonVersion();

        if (environment is not null)
        {
            EnsureVersionMatches(expectedPythonVersion);
            return;
        }

        var shouldLogWaiting = initializationSemaphore.CurrentCount == 0;

        if (shouldLogWaiting)
        {
            logger.Log($"Waiting for shared Python runtime initialization ({expectedPythonVersion})", LogColors.PaleChestnut);
        }

        await initializationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (environment is not null)
            {
                EnsureVersionMatches(expectedPythonVersion);
                return;
            }

            // CSnakes/CPython bootstrap is thread-sensitive, so initialize inline on the
            // caller that won the semaphore instead of hopping to a worker thread.
            cancellationToken.ThrowIfCancellationRequested();
            InitializeCore(logger, expectedPythonVersion);
        }
        finally
        {
            initializationSemaphore.Release();
        }

        EnsureVersionMatches(expectedPythonVersion);
    }

    private void InitializeCore(IBotLogger logger, string expectedPythonVersion)
    {
        Directory.CreateDirectory(scriptsPath);
        // The source-generated wrapper imports the bridge by module name, so make sure a
        // concrete file exists in the Scripts root used by the resolved interpreter.
        EnsureBridgeModuleFile();

        var venvPath = Path.Combine(scriptsPath, ".venv");
        var hasVirtualEnvironment = Directory.Exists(venvPath);

        logger.Log(
            hasVirtualEnvironment
                ? $"Initializing Python runtime from virtual environment at '{venvPath}'"
                : $"Initializing Python runtime {DefaultRedistributablePythonVersion} from redistributable. Python may be downloaded on first use."
            , LogColors.PaleChestnut);

        var services = new ServiceCollection();
        var pythonBuilder = services
            .WithPython()
            .WithHome(scriptsPath);

        if (hasVirtualEnvironment)
        {
            var basePythonHome = GetVirtualEnvironmentBaseHome(venvPath);
            pythonBuilder = pythonBuilder
                .FromFolder(basePythonHome, expectedPythonVersion)
                .WithVirtualEnvironment(venvPath);
        }
        else
        {
            pythonBuilder = pythonBuilder.FromRedistributable(DefaultRedistributablePythonVersion);
        }

        serviceProvider = services.BuildServiceProvider();
        environment = serviceProvider.GetRequiredService<IPythonEnvironment>();

        actualPythonVersion = ExtractMajorMinorVersion(environment.Version.ToString());
        logger.Log($"Python runtime ready: {actualPythonVersion}", LogColors.PaleChestnut);
        EnsureVersionMatches(expectedPythonVersion);
    }

    private void EnsureVersionMatches(string requestedPythonVersion)
    {
        var normalizedRequestedVersion = NormalizePythonVersion(requestedPythonVersion);

        if (!string.Equals(actualPythonVersion, normalizedRequestedVersion, StringComparison.Ordinal))
        {
            throw new BlockExecutionException(
                $"The Python runtime resolved to version {actualPythonVersion}, but this OpenBullet2 process expects {normalizedRequestedVersion}. Restart OpenBullet2 before switching Python environments.");
        }
    }

    private static string NormalizePythonVersion(string pythonVersion)
    {
        var trimmed = pythonVersion.Trim();

        if (!Regex.IsMatch(trimmed, @"^\d+\.\d+$"))
        {
            throw new BlockExecutionException(
                $"Invalid Python version '{pythonVersion}'. Expected a major.minor format like 3.12");
        }

        return trimmed;
    }

    private static string ExtractMajorMinorVersion(string value)
    {
        var match = Regex.Match(value, @"^(\d+\.\d+)");

        if (!match.Success)
        {
            throw new BlockExecutionException($"Could not determine the Python version from '{value}'");
        }

        return match.Groups[1].Value;
    }

    private string ResolveExpectedPythonVersion()
    {
        var venvPath = Path.Combine(scriptsPath, ".venv");

        if (!Directory.Exists(venvPath))
        {
            return DefaultRedistributablePythonVersion;
        }

        return GetVirtualEnvironmentVersion(venvPath);
    }

    private static string GetVirtualEnvironmentBaseHome(string venvPath)
    {
        var configPath = Path.Combine(venvPath, "pyvenv.cfg");

        if (!File.Exists(configPath))
        {
            throw new BlockExecutionException(
                $"The virtual environment at '{venvPath}' is missing pyvenv.cfg");
        }

        var homeLine = File.ReadLines(configPath)
            .FirstOrDefault(line => line.StartsWith("home =", StringComparison.OrdinalIgnoreCase));

        if (homeLine is null)
        {
            throw new BlockExecutionException(
                $"The virtual environment at '{venvPath}' does not declare a base Python home");
        }

        var baseHome = homeLine.Split('=', 2)[1].Trim();

        if (string.IsNullOrWhiteSpace(baseHome) || !Directory.Exists(baseHome))
        {
            throw new BlockExecutionException(
                $"The virtual environment at '{venvPath}' points to an invalid base Python home '{baseHome}'");
        }

        return baseHome;
    }

    private static string GetVirtualEnvironmentVersion(string venvPath)
    {
        var configPath = Path.Combine(venvPath, "pyvenv.cfg");

        if (!File.Exists(configPath))
        {
            throw new BlockExecutionException(
                $"The virtual environment at '{venvPath}' is missing pyvenv.cfg");
        }

        var versionLine = File.ReadLines(configPath)
            .FirstOrDefault(line => line.StartsWith("version_info =", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("version =", StringComparison.OrdinalIgnoreCase));

        if (versionLine is null)
        {
            throw new BlockExecutionException(
                $"The virtual environment at '{venvPath}' does not declare version_info or version");
        }

        return ExtractMajorMinorVersion(versionLine.Split('=', 2)[1].Trim());
    }

    private void LogCapturedOutput(IBotLogger logger, string invocationId)
    {
        if (environment is null)
        {
            return;
        }

        var (stdout, stderr) = environment.Ob2PythonBridge().TakeLogs(invocationId);
        LogCapturedStream(logger, stdout, "[Python stdout] ", LogColors.PaleChestnut);
        LogCapturedStream(logger, stderr, "[Python stderr] ", LogColors.Tomato);
    }

    private static void LogCapturedStream(IBotLogger logger, string text, string prefix, string color)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        foreach (var line in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                logger.Log(prefix + line, color);
            }
        }
    }

    private void EnsureBridgeModuleFile()
    {
        var bridgePath = Path.Combine(scriptsPath, BridgeModuleFileName);
        using var stream = typeof(PythonScriptRuntime).Assembly.GetManifestResourceStream(BridgeModuleResourceName)
            ?? throw new BlockExecutionException(
                $"Could not find the embedded Python bridge resource '{BridgeModuleResourceName}'");
        using var reader = new StreamReader(stream);
        File.WriteAllText(bridgePath, reader.ReadToEnd());
    }

    private static IReadOnlyDictionary<string, PyObject> BuildInputDictionary(string[] inputNames, object[] inputValues)
    {
        if (inputNames.Length != inputValues.Length)
        {
            throw new BlockExecutionException(
                $"The Python script expected {inputNames.Length} inputs but received {inputValues.Length} values");
        }

        var dictionary = new Dictionary<string, PyObject>(inputNames.Length, StringComparer.Ordinal);

        for (var i = 0; i < inputNames.Length; i++)
        {
            dictionary[inputNames[i]] = SerializeValue(inputValues[i]);
        }

        return dictionary;
    }

    private static PyObject SerializeValue(object? value)
        => value switch
        {
            null => throw new BlockExecutionException("Python script inputs cannot be null"),
            bool boolean => PyObject.From(boolean),
            byte number => PyObject.From((long)number),
            sbyte number => PyObject.From((long)number),
            short number => PyObject.From((long)number),
            ushort number => PyObject.From((long)number),
            int number => PyObject.From((long)number),
            uint number => PyObject.From((long)number),
            long number => PyObject.From(number),
            ulong number => PyObject.From((double)number),
            float number => PyObject.From((double)number),
            double number => PyObject.From(number),
            decimal number => PyObject.From((double)number),
            string text => PyObject.From(text),
            byte[] bytes => PyObject.From(bytes),
            IEnumerable<string> list => PyObject.From(list.ToArray()),
            IDictionary<string, string> dictionary => PyObject.From(dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)),
            _ => throw new BlockExecutionException(
                $"The Python script input type '{value.GetType()}' is not supported. Serialize custom values to strings before passing them to Python.")
        };

    private static IReadOnlyDictionary<string, object> ConvertOutputs(
        IReadOnlyDictionary<string, PyObject> result,
        string[] outputNames,
        VariableType[] outputTypes)
    {
        if (outputNames.Length != outputTypes.Length)
        {
            throw new BlockExecutionException(
                $"The Python script returned {outputNames.Length} output names but {outputTypes.Length} output types were provided");
        }

        var converted = new Dictionary<string, object>(outputNames.Length, StringComparer.Ordinal);

        try
        {
            for (var i = 0; i < outputNames.Length; i++)
            {
                var outputName = outputNames[i];

                if (!result.TryGetValue(outputName, out var value))
                {
                    throw new BlockExecutionException($"Python output '{outputName}' is missing");
                }

                converted[outputName] = ConvertOutputValue(outputName, outputTypes[i], value);
            }

            return converted;
        }
        finally
        {
            foreach (var value in result.Values)
            {
                value.Dispose();
            }
        }
    }

    private static object ConvertOutputValue(string outputName, VariableType outputType, PyObject value)
        => outputType switch
        {
            VariableType.Bool => value.As<bool>(),
            VariableType.ByteArray => value.As<byte[]>(),
            VariableType.Float => value.As<double>(),
            VariableType.Int => value.As<long>(),
            VariableType.String => value.As<string>(),
            VariableType.ListOfStrings => value.As<IReadOnlyList<string>>().ToList(),
            VariableType.DictionaryOfStrings => value.As<IReadOnlyDictionary<string, string>>()
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            _ => throw new BlockExecutionException(
                $"Python output '{outputName}' uses unsupported OpenBullet type '{outputType}'")
        };

    private static string FormatResultForLog(IReadOnlyDictionary<string, object> result)
        => "{" + string.Join(", ", result.Select(kvp => $"{kvp.Key} = {FormatValueForLog(kvp.Value)}")) + "}";

    private static string FormatValueForLog(object? value)
        => value switch
        {
            null => "null",
            byte[] bytes => $"byte[{bytes.Length}]",
            IEnumerable<string> strings => "[" + string.Join(", ", strings) + "]",
            IReadOnlyDictionary<string, object> dictionary => "{" + string.Join(", ", dictionary.Select(kvp => $"{kvp.Key}: {FormatValueForLog(kvp.Value)}")) + "}",
            IEnumerable<object> objects => "[" + string.Join(", ", objects.Select(FormatValueForLog)) + "]",
            _ => value.ToString() ?? string.Empty
        };

    public void Dispose()
    {
        if (sharedRuntime.IsValueCreated && ReferenceEquals(this, sharedRuntime.Value))
        {
            return;
        }

        initializationSemaphore.Dispose();
        serviceProvider?.Dispose();
        serviceProvider = null;
        environment = null;
    }
}
