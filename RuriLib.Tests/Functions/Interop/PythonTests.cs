using RuriLib.Blocks.Interop;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace RuriLib.Tests.Functions.Interop;

using BotProviders = RuriLib.Models.Bots.Providers;

public class PythonTests
{
    private static readonly string TempRoot = Path.Combine(Path.GetTempPath(), $"{nameof(PythonTests)}-{Guid.NewGuid():N}");
    private static readonly string ScriptsPath = Path.Combine(TempRoot, "Scripts");
    private static readonly string VenvPath = Path.Combine(ScriptsPath, ".venv");

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
    }

    [Fact]
    public void InvokePython_StringOutputsFromSyncCode_ReturnsJson()
    {
        var script = "result = DATA + suffix";
        var data = CreateBotData();

        var result = Methods.InvokePython(
            data,
            script,
            GetScriptHash(script),
            ["DATA", "suffix"],
            ["hello", "_world"],
            ["result"],
            "3.12");

        Assert.Equal("hello_world", result.GetProperty("result").GetString());
    }

    [Fact]
    public void InvokePython_ListDictionaryAndBytesOutputs_ReturnsNormalizedJson()
    {
        var script = """
result = ["a", "b"]
mapping = {"x": "1", "y": "2"}
payload = b"hello"
""";
        var data = CreateBotData();

        var result = Methods.InvokePython(
            data,
            script,
            GetScriptHash(script),
            [],
            [],
            ["result", "mapping", "payload"],
            "3.12");

        Assert.Equal("a", result.GetProperty("result")[0].GetString());
        Assert.Equal("2", result.GetProperty("mapping").GetProperty("y").GetString());
        Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes("hello")), result.GetProperty("payload").GetString());
    }

    [Fact]
    public void InvokePython_AsyncCode_ReturnsJson()
    {
        var script = """
import asyncio

await asyncio.sleep(0)
result = DATA + "_async"
""";
        var data = CreateBotData();

        var result = Methods.InvokePython(
            data,
            script,
            GetScriptHash(script),
            ["DATA"],
            ["hello"],
            ["result"],
            "3.12");

        Assert.Equal("hello_async", result.GetProperty("result").GetString());
    }

    private static BotData CreateBotData()
        => new(
            new BotProviders(null!),
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()));

    private static string GetScriptHash(string script)
        => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(script))).ToLowerInvariant();
}
