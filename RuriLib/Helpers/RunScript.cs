using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RuriLib.Helpers;

/// <summary>
/// Runs local scripts and captures their standard output.
/// </summary>
public static class RunScript
{
    /// <summary>
    /// Runs a supported script file and returns its standard output.
    /// </summary>
    /// <param name="scriptPath">The script path.</param>
    /// <returns>The standard output, or <c>null</c> if unsupported or failed.</returns>
    public static async Task<string?> RunScriptAndGetStdOut(string scriptPath)
    {
        ArgumentNullException.ThrowIfNull(scriptPath);

        var startInfo = CreateStartInfo(scriptPath);
        if (startInfo is null)
        {
            // un-supported script.
            return null;
        }

        try
        {
            using var process = new Process { StartInfo = startInfo };

            process.Start();
            var stdOutTask = process.StandardOutput.ReadToEndAsync();
            var stdErrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync().ConfigureAwait(false);
            var stdOut = await stdOutTask.ConfigureAwait(false);
            await stdErrTask.ConfigureAwait(false);

            return stdOut.Length > 0 ? stdOut : null;
        }
        catch
        {
            // ignored
            return null;
        }
    }

    private static ProcessStartInfo? CreateStartInfo(string scriptPath)
    {
        var fileExtension = Path.GetExtension(scriptPath).ToLowerInvariant();

        return fileExtension switch
        {
            ".bat" => CreateCmdStartInfo(scriptPath),
            ".ps1" => CreatePowerShellStartInfo(scriptPath),
            ".sh" => CreateBashStartInfo(scriptPath),
            _ => null
        };
    }

    private static ProcessStartInfo CreateCmdStartInfo(string scriptPath)
        => new("cmd.exe", $@"/C ""{scriptPath}""")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

    private static ProcessStartInfo CreatePowerShellStartInfo(string scriptPath)
        => new("powershell.exe", $@"-NoProfile -ExecutionPolicy Bypass -File ""{scriptPath}""")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

    private static ProcessStartInfo CreateBashStartInfo(string scriptPath)
        => new("/bin/bash", scriptPath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
}
