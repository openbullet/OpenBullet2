using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RuriLib.Helpers;
using Xunit;

namespace RuriLib.Tests.Helpers;

public class RunScriptTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task RunScriptAndGetStdOut_UnsupportedExtension_ReturnsNull()
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(scriptPath, "hello", Encoding.UTF8, TestCancellationToken);

        try
        {
            var stdout = await RunScript.RunScriptAndGetStdOut(scriptPath);

            Assert.Null(stdout);
        }
        finally
        {
            File.Delete(scriptPath);
        }
    }

    [Fact]
    public async Task RunScriptAndGetStdOut_BatchFile_ReturnsStdOut()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var scriptPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bat");
        await File.WriteAllTextAsync(scriptPath, "@echo off\r\necho first\r\necho second\r\n", Encoding.ASCII, TestCancellationToken);

        try
        {
            var stdout = await RunScript.RunScriptAndGetStdOut(scriptPath);

            Assert.Equal($"first{Environment.NewLine}second{Environment.NewLine}", stdout);
        }
        finally
        {
            File.Delete(scriptPath);
        }
    }

    [Fact]
    public async Task RunScriptAndGetStdOut_PowerShellFile_ReturnsStdOut()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var scriptPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.ps1");
        await File.WriteAllTextAsync(scriptPath, "Write-Output 'first'\r\nWrite-Output 'second'\r\n", Encoding.UTF8, TestCancellationToken);

        try
        {
            var stdout = await RunScript.RunScriptAndGetStdOut(scriptPath);

            Assert.Equal($"first{Environment.NewLine}second{Environment.NewLine}", stdout);
        }
        finally
        {
            File.Delete(scriptPath);
        }
    }
}
