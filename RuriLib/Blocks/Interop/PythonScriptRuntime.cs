using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using RuriLib.Exceptions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Interop;

internal sealed class PythonScriptRuntime : IDisposable
{
    private const string BridgeModuleFileName = "ob2_python_bridge.py";
    private const string BridgeModuleResourceName = "RuriLib.Blocks.Interop.ob2_python_bridge.py";
    private readonly object syncObject = new();
    private readonly string scriptsPath;

    private ServiceProvider? serviceProvider;
    private IPythonEnvironment? environment;
    private string? actualPythonVersion;

    public PythonScriptRuntime(string scriptsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptsPath);
        this.scriptsPath = Path.GetFullPath(scriptsPath);
    }

    public async Task<JsonElement> InvokeAsync(
        BotData data,
        string script,
        string scriptHash,
        string[] inputNames,
        object[] inputValues,
        string[] outputNames,
        string pythonVersion)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(script);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(pythonVersion);
        ArgumentNullException.ThrowIfNull(inputNames);
        ArgumentNullException.ThrowIfNull(inputValues);
        ArgumentNullException.ThrowIfNull(outputNames);

        InitializeIfNeeded(pythonVersion);
        data.CancellationToken.ThrowIfCancellationRequested();

        var json = await environment!
            .Ob2PythonBridge()
            .Run(
                scriptHash,
                script,
                JsonSerializer.Serialize(SerializeInputs(inputNames, inputValues)),
                JsonSerializer.Serialize(outputNames),
                data.CancellationToken)
            .ConfigureAwait(false);

        data.Logger.Log($"Executed Python script with result: {json}", LogColors.PaleChestnut);

        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private void InitializeIfNeeded(string requestedPythonVersion)
    {
        lock (syncObject)
        {
            if (environment is not null)
            {
                EnsureVersionMatches(requestedPythonVersion);
                return;
            }

            Directory.CreateDirectory(scriptsPath);
            // The source-generated wrapper imports the bridge by module name, so make sure a
            // concrete file exists in the Scripts root used by the resolved interpreter.
            EnsureBridgeModuleFile();

            var venvPath = Path.Combine(scriptsPath, ".venv");
            var normalizedPythonVersion = NormalizePythonVersion(requestedPythonVersion);

            var services = new ServiceCollection();
            var pythonBuilder = services
                .WithPython()
                .WithHome(scriptsPath);

            if (Directory.Exists(venvPath))
            {
                var basePythonHome = GetVirtualEnvironmentBaseHome(venvPath);
                pythonBuilder = pythonBuilder
                    .FromFolder(basePythonHome, normalizedPythonVersion)
                    .WithVirtualEnvironment(venvPath);
            }
            else
            {
                pythonBuilder = pythonBuilder.FromRedistributable(normalizedPythonVersion);
            }

            serviceProvider = services.BuildServiceProvider();
            environment = serviceProvider.GetRequiredService<IPythonEnvironment>();

            actualPythonVersion = ExtractMajorMinorVersion(environment.Version.ToString());

            EnsureVersionMatches(requestedPythonVersion);
        }
    }

    private void EnsureVersionMatches(string requestedPythonVersion)
    {
        var normalizedRequestedVersion = NormalizePythonVersion(requestedPythonVersion);

        if (!string.Equals(actualPythonVersion, normalizedRequestedVersion, StringComparison.Ordinal))
        {
            throw new BlockExecutionException(
                $"The Python runtime resolved to version {actualPythonVersion}, but the block requested {normalizedRequestedVersion}");
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

    private void EnsureBridgeModuleFile()
    {
        var bridgePath = Path.Combine(scriptsPath, BridgeModuleFileName);
        using var stream = typeof(PythonScriptRuntime).Assembly.GetManifestResourceStream(BridgeModuleResourceName)
            ?? throw new BlockExecutionException(
                $"Could not find the embedded Python bridge resource '{BridgeModuleResourceName}'");
        using var reader = new StreamReader(stream);
        File.WriteAllText(bridgePath, reader.ReadToEnd());
    }

    private static IEnumerable<InputValueDto> SerializeInputs(string[] inputNames, object[] inputValues)
    {
        if (inputNames.Length != inputValues.Length)
        {
            throw new BlockExecutionException(
                $"The Python script expected {inputNames.Length} inputs but received {inputValues.Length} values");
        }

        for (var i = 0; i < inputNames.Length; i++)
        {
            yield return new InputValueDto(inputNames[i], SerializeValue(inputValues[i]));
        }
    }

    private static object? SerializeValue(object? value)
        => value switch
        {
            null => null,
            bool or byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal or string => value,
            // JSON cannot represent raw bytes, so tag them and let the Python bridge decode them.
            byte[] bytes => new TaggedValueDto("bytes", Convert.ToBase64String(bytes)),
            IEnumerable<string> list => list.ToArray(),
            IDictionary<string, string> dictionary => dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            _ => throw new BlockExecutionException(
                $"The Python script input type '{value.GetType()}' is not supported for CSnakes bridge serialization")
        };

    private sealed record InputValueDto(string Name, object? Value);

    private sealed record TaggedValueDto(string __ob2_type__, string value);

    public void Dispose()
    {
        serviceProvider?.Dispose();
        serviceProvider = null;
        environment = null;
    }
}
