using IronPython.Hosting;
using Jint;
using Microsoft.Scripting.Hosting;
using RuriLib.Exceptions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;
using InteropMethods = RuriLib.Blocks.Interop.Methods;

namespace RuriLib.Tests.Blocks.Interop;

public class InteropBlocksTests
{
    [Fact]
    public async Task ShellCommand_DotnetVersion_ReturnsOutput()
    {
        var data = NewBotData();

        var output = await InteropMethods.ShellCommandAsync(data, "dotnet", "--version");

        Assert.False(string.IsNullOrWhiteSpace(output));
        Assert.Contains(".", output);
    }

    [Fact]
    public async Task ShellCommand_Timeout_ThrowsTimeoutException()
    {
        var data = NewBotData();
        var (executable, arguments) = GetSleepCommand();

        await Assert.ThrowsAsync<TimeoutException>(() =>
            InteropMethods.ShellCommandAsync(data, executable, arguments, timeoutMilliseconds: 100));
    }

    [Fact]
    public void InvokeJint_ScriptFile_ExecutesAndReturnsEngine()
    {
        var data = NewBotData();
        var engine = new Engine();
        var tempFile = Path.Combine(Path.GetTempPath(), $"{nameof(InteropBlocksTests)}-{Guid.NewGuid():N}.js");

        try
        {
            File.WriteAllText(tempFile, "var result = 2 + 3;");

#pragma warning disable CS0618
            var resultEngine = InteropMethods.InvokeJint(data, engine, tempFile);
#pragma warning restore CS0618

            Assert.Equal(5, resultEngine.GetValue("result").AsNumber());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetIronPyScope_WithoutEngine_Throws()
    {
        var data = NewBotData();

#pragma warning disable CS0618
        Assert.Throws<BlockExecutionException>(() => InteropMethods.GetIronPyScope(data));
#pragma warning restore CS0618
    }

    [Fact]
    public void GetIronPyScope_WithEngine_ReturnsScope()
    {
        var data = NewBotData();
        var engine = Python.CreateEngine();
        data.SetObject("ironPyEngine", engine);

#pragma warning disable CS0618
        var scope = InteropMethods.GetIronPyScope(data);
#pragma warning restore CS0618

        Assert.IsType<ScriptScope>(scope);
    }

    private static BotData NewBotData()
        => new(
            new BotProviders(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()));

    private static (string Executable, string Arguments) GetSleepCommand()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ("powershell.exe", "-NoProfile -Command \"Start-Sleep -Seconds 5\"");
        }

        return ("/bin/sh", "-c \"sleep 5\"");
    }
}
