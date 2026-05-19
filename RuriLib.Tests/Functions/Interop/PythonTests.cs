using RuriLib.Blocks.Interop;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Functions.Interop;

using BotProviders = RuriLib.Models.Bots.Providers;

public class PythonTests
{
    private static readonly string TempRoot = Path.Combine(Path.GetTempPath(), $"{nameof(PythonTests)}-{Guid.NewGuid():N}");
    private static readonly string ScriptsPath = Path.Combine(TempRoot, "Scripts");
    private static readonly string VenvPath = Path.Combine(ScriptsPath, ".venv");
    private static readonly string VenvPythonVersion;

    static PythonTests()
    {
        Directory.CreateDirectory(ScriptsPath);

        if (!Directory.Exists(VenvPath))
        {
            var startInfo = new ProcessStartInfo("python", $"-m venv \"{VenvPath}\"")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start python to create the test virtual environment");
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Failed to create the test virtual environment. Stdout: {process.StandardOutput.ReadToEnd()} Stderr: {process.StandardError.ReadToEnd()}");
            }
        }

        var versionLine = File.ReadLines(Path.Combine(VenvPath, "pyvenv.cfg"))
            .First(line => line.StartsWith("version_info =", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("version =", StringComparison.OrdinalIgnoreCase));
        var versionParts = versionLine.Split('=', 2)[1].Trim().Split('.');
        VenvPythonVersion = $"{versionParts[0]}.{versionParts[1]}";
    }

    [Fact]
    public async Task InvokePython_StringOutputsFromSyncCode_ReturnsTypedOutputs()
    {
        var script = "result = DATA + suffix";
        var logger = new BotLogger();
        var data = CreatePythonBotData(TestContext.Current.CancellationToken, logger);

        var result = await Methods.InvokePythonAsync(
            data,
            script,
            GetScriptHash(script),
            ["DATA", "suffix"],
            ["hello", "_world"],
            ["result"],
            [VariableType.String]).WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal("hello_world", result["result"]);
        Assert.Contains(logger.Entries, entry =>
            entry.Color == LogColors.PaleChestnut
            && entry.Message.Contains("Initializing Python runtime from virtual environment", StringComparison.Ordinal));
        Assert.Contains(logger.Entries, entry =>
            entry.Color == LogColors.PaleChestnut
            && entry.Message.Contains($"Python runtime ready: {VenvPythonVersion}", StringComparison.Ordinal));
    }

    [Fact]
    public async Task InvokePython_ListDictionaryAndBytesOutputs_ReturnsTypedOutputs()
    {
        var script = """
result = ["a", "b"]
mapping = {"x": "1", "y": "2"}
payload = b"hello"
""";
        var data = CreatePythonBotData(TestContext.Current.CancellationToken);

        var result = await Methods.InvokePythonAsync(
            data,
            script,
            GetScriptHash(script),
            [],
            [],
            ["result", "mapping", "payload"],
            [VariableType.ListOfStrings, VariableType.DictionaryOfStrings, VariableType.ByteArray]).WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal("a", Assert.IsAssignableFrom<IReadOnlyList<string>>(result["result"])[0]);
        Assert.Equal("2", Assert.IsType<Dictionary<string, string>>(result["mapping"])["y"]);
        Assert.Equal(Encoding.UTF8.GetBytes("hello"), Assert.IsType<byte[]>(result["payload"]));
    }

    [Fact]
    public async Task InvokePython_AsyncCode_ReturnsTypedOutputs()
    {
        var script = """
import asyncio

await asyncio.sleep(0)
result = DATA + "_async"
""";
        var data = CreatePythonBotData(TestContext.Current.CancellationToken);

        var result = await Methods.InvokePythonAsync(
            data,
            script,
            GetScriptHash(script),
            ["DATA"],
            ["hello"],
            ["result"],
            [VariableType.String]).WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal("hello_async", result["result"]);
    }

    [Fact]
    public async Task InvokePython_AsyncCode_PropagatesCancellationIntoPython()
    {
        var markerPath = Path.Combine(TempRoot, $"{Guid.NewGuid():N}.txt");
        var script = """
import asyncio

try:
    await asyncio.sleep(30)
    result = "completed"
except asyncio.CancelledError:
    with open(marker_path, "w", encoding="utf-8") as handle:
        handle.write("cancelled")
    raise
""";

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));
        var data = CreatePythonBotData(cts.Token);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Methods.InvokePythonAsync(
            data,
            script,
            GetScriptHash(script),
            ["marker_path"],
            [markerPath],
            ["result"],
            [VariableType.String]).WaitAsync(TestContext.Current.CancellationToken));

        Assert.True(
            await WaitForConditionAsync(() => File.Exists(markerPath), TimeSpan.FromSeconds(3)),
            "Expected the Python coroutine to observe cancellation and write the marker file");
    }

    [Fact]
    public async Task InvokePython_WhenScriptFails_PropagatesExceptionAndLogsError()
    {
        var script = """
raise ValueError("boom")
""";
        var logger = new BotLogger();
        var data = CreatePythonBotData(TestContext.Current.CancellationToken, logger);

        var exception = await Assert.ThrowsAnyAsync<Exception>(() => Methods.InvokePythonAsync(
            data,
            script,
            GetScriptHash(script),
            [],
            [],
            ["result"],
            [VariableType.String]).WaitAsync(TestContext.Current.CancellationToken));

        Assert.Contains("boom", exception.ToString(), StringComparison.Ordinal);
        Assert.Contains(logger.Entries, entry =>
            entry.Color == LogColors.Tomato
            && entry.Message.Contains("Python script failed:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task InvokePython_MissingModule_FailsOnRepeatedCallsWithSharedRuntime()
    {
        var script = """
import definitely_missing_package_12345
result = 1
""";

        var sharedRuntime = PythonScriptRuntime.GetShared();
        var firstData = CreateBotData(TestContext.Current.CancellationToken);
        firstData.SetObject("pythonRuntime", sharedRuntime, false);

        var firstException = await Assert.ThrowsAnyAsync<Exception>(() => Methods.InvokePythonAsync(
            firstData,
            script,
            GetScriptHash(script),
            [],
            [],
            ["result"],
            [VariableType.Int]).WaitAsync(TestContext.Current.CancellationToken));

        var firstError = firstException.Message;

        var secondData = CreateBotData(TestContext.Current.CancellationToken);
        secondData.SetObject("pythonRuntime", sharedRuntime, false);

        var secondException = await Assert.ThrowsAnyAsync<Exception>(() => Methods.InvokePythonAsync(
            secondData,
            script,
            GetScriptHash(script),
            [],
            [],
            ["result"],
            [VariableType.Int]).WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));

        var secondError = secondException.Message;
        Assert.Equal(firstException.GetType(), secondException.GetType());
        Assert.Equal(firstError, secondError);
    }

    private static BotData CreateBotData(CancellationToken cancellationToken = default, BotLogger? logger = null)
        => new(
            new BotProviders(null!),
            new ConfigSettings(),
            logger ?? new BotLogger(),
            new DataLine("hello", new WordlistType()))
        {
            CancellationToken = cancellationToken
        };

    private static BotData CreatePythonBotData(CancellationToken cancellationToken = default, BotLogger? logger = null)
    {
        var data = CreateBotData(cancellationToken, logger);
        data.SetObject("pythonRuntime", new PythonScriptRuntime(ScriptsPath));
        return data;
    }

    private static string GetScriptHash(string script)
        => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(script))).ToLowerInvariant();

    private static async Task<bool> WaitForConditionAsync(Func<bool> predicate, TimeSpan timeout)
    {
        var start = Stopwatch.StartNew();

        while (start.Elapsed < timeout)
        {
            if (predicate())
            {
                return true;
            }

            await Task.Delay(50, TestContext.Current.CancellationToken);
        }

        return predicate();
    }
}
