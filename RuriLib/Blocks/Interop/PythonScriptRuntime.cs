using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using Microsoft.Extensions.DependencyInjection;
using RuriLib.Exceptions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RuriLib.Blocks.Interop;

internal sealed class PythonScriptRuntime : IDisposable
{
    private const string BridgeModuleName = "__ob2_python_bridge__";
    private readonly object syncObject = new();
    private readonly string scriptsPath;

    private ServiceProvider? serviceProvider;
    private IPythonEnvironment? environment;
    private PyObject? bridgeModule;
    private string? actualPythonVersion;

    public PythonScriptRuntime(string scriptsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptsPath);
        this.scriptsPath = Path.GetFullPath(scriptsPath);
    }

    public JsonElement Invoke(
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

        using (GIL.Acquire())
        {
            using var runFunction = bridgeModule!.GetAttr("run");
            using PyObject scriptHashObject = PyObject.From(scriptHash);
            using PyObject scriptObject = PyObject.From(script);
            using PyObject inputNamesObject = PyObject.From(inputNames);
            using PyObject inputValuesObject = PyObject.From(inputValues);
            using PyObject outputNamesObject = PyObject.From(outputNames);
            using var result = runFunction.Call(
                scriptHashObject,
                scriptObject,
                inputNamesObject,
                inputValuesObject,
                outputNamesObject);

            var json = result.As<string>();
            data.Logger.Log($"Executed Python script with result: {json}", LogColors.PaleChestnut);

            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
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
            WriteBridgeModule();

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

            using (GIL.Acquire())
            {
                EnsureScriptsPathOnSysPath();
                bridgeModule = Import.ImportModule(BridgeModuleName);
                actualPythonVersion = GetPythonVersion();
            }

            EnsureVersionMatches(requestedPythonVersion);
        }
    }

    private void EnsureScriptsPathOnSysPath()
    {
        using var importlib = Import.ImportModule("importlib");
        using var invalidateCaches = importlib.GetAttr("invalidate_caches");
        invalidateCaches.Call();

        using var sys = Import.ImportModule("sys");
        using var path = sys.GetAttr("path");
        using var append = path.GetAttr("append");
        using PyObject scriptsPathObject = PyObject.From(scriptsPath);
        append.Call(scriptsPathObject);
    }

    private string GetPythonVersion()
    {
        using var sys = Import.ImportModule("sys");
        using var version = sys.GetAttr("version");
        var text = version.ToString();
        var match = Regex.Match(text, @"^(\d+\.\d+)");

        if (!match.Success)
        {
            throw new BlockExecutionException($"Could not determine the Python version from '{text}'");
        }

        return match.Groups[1].Value;
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

    private void WriteBridgeModule()
    {
        var bridgePath = Path.Combine(scriptsPath, $"{BridgeModuleName}.py");

        if (File.Exists(bridgePath))
        {
            return;
        }

        File.WriteAllText(bridgePath, """
import asyncio
import base64
import json

_CACHE = {}

def _indent(script_source):
    lines = script_source.splitlines()
    if not lines:
        return "    pass"

    return "\n".join(("    " + line) if line else "" for line in lines)

def _make_return_block(output_names):
    if not output_names:
        return "    return {}"

    lines = ["    return {"]
    for name in output_names:
        lines.append(f"        {name!r}: {name},")
    lines.append("    }")
    return "\n".join(lines)

def _normalize(value):
    if value is None or isinstance(value, (str, int, float, bool)):
        return value

    if isinstance(value, (bytes, bytearray, memoryview)):
        return base64.b64encode(bytes(value)).decode("ascii")

    if isinstance(value, dict):
        return {str(k): _normalize(v) for k, v in value.items()}

    if isinstance(value, (list, tuple, set)):
        return [_normalize(v) for v in value]

    return value

def run(script_hash, script_source, input_names, input_values, output_names):
    fn = _CACHE.get(script_hash)

    if fn is None:
        args = ", ".join(input_names)
        wrapper = "\n".join([
            f"async def __ob2_entry__({args}):",
            _indent(script_source),
            _make_return_block(output_names)
        ])

        namespace = {}
        exec(compile(wrapper, f"<ob2:{script_hash}>", "exec"), namespace, namespace)
        fn = namespace["__ob2_entry__"]
        _CACHE[script_hash] = fn

    kwargs = dict(zip(input_names, input_values))
    result = asyncio.run(fn(**kwargs))
    return json.dumps(_normalize(result))
""");
    }

    public void Dispose()
    {
        bridgeModule?.Dispose();
        bridgeModule = null;
        serviceProvider?.Dispose();
        serviceProvider = null;
        environment = null;
    }
}
