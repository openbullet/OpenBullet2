using System.Diagnostics;
using System.Runtime.InteropServices;
using CliWrap;
using CliWrap.Buffered;
using Spectre.Console;

namespace OpenBullet2.Updater.Core.Helpers;

public static class UpdaterRequirements
{
    public static async Task EnsureProcessNotRunningAsync(string processName)
    {
        var isRunning = false;

        await AnsiConsole.Status()
            .StartAsync("[yellow]Checking if OpenBullet 2 is running...[/]", async ctx =>
            {
                // Wait for 10 seconds for the process to close
                var timeout = TimeSpan.FromSeconds(10);
                while (Process.GetProcessesByName(processName).Length > 0 && timeout > TimeSpan.Zero)
                {
                    ctx.Status("[yellow]OpenBullet 2 is running, waiting for it to close...[/]")
                        .Spinner(Spinner.Known.Dots)
                        .Refresh();

                    await Task.Delay(1000);
                    timeout -= TimeSpan.FromSeconds(1);
                }

                if (Process.GetProcessesByName(processName).Length > 0)
                {
                    isRunning = true;
                }
            });

        if (isRunning)
        {
            Utils.ExitWithError("OpenBullet 2 is currently running, please close it before updating!");
        }
    }

    public static async Task<bool> IsRuntimeInstalledAsync(string runtimeName, Version dotnetVersion)
    {
        try
        {
            // If dotnet is not a valid command, this will throw an exception
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-runtimes")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return result.ExitCode == 0 && result.StandardOutput.Split('\n').Any(line =>
                line.StartsWith($"{runtimeName} {dotnetVersion.Major}."));
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> IsRuntimeOrSdkInstalledAsync(string runtimeName, Version dotnetVersion)
        => await IsRuntimeInstalledAsync(runtimeName, dotnetVersion)
           || await IsSdkInstalledAsync(dotnetVersion);

    public static async Task<bool> IsSdkInstalledAsync(Version dotnetVersion)
    {
        try
        {
            // If dotnet is not a valid command, this will throw an exception
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-sdks")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return result.ExitCode == 0 && result.StandardOutput.Split('\n').Any(line =>
                line.StartsWith($"{dotnetVersion.Major}."));
        }
        catch
        {
            return false;
        }
    }

    public static async Task InstallRuntimeAsync(string runtimeFileName, Version dotnetVersion)
    {
        var downloadUrl = $"https://aka.ms/dotnet/{dotnetVersion}/{runtimeFileName}";

        await using var runtimeStream = await DownloadRuntimeAsync(downloadUrl);

        var tempPath = Path.GetTempFileName() + ".exe";
        await using (var tempStream = File.OpenWrite(tempPath))
        {
            await runtimeStream.CopyToAsync(tempStream);
        }

        await Cli.Wrap("cmd")
            .WithArguments($"/c {tempPath}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
    }

    public static string GetRuntimeFileName(string runtimePrefix)
    {
        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.Arm64 => $"{runtimePrefix}-win-arm64.exe",
            Architecture.X64 => $"{runtimePrefix}-win-x64.exe",
            Architecture.X86 => $"{runtimePrefix}-win-x86.exe",
            _ => throw new NotImplementedException()
        };
    }

    private static async Task<Stream> DownloadRuntimeAsync(string url)
    {
        return await AnsiConsole.Progress()
            .Columns([
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn()
            ])
            .StartAsync(async ctx =>
            {
                var downloadTask = ctx.AddTask("[green]Downloading[/]");

                var progress = new Progress<double>(p =>
                {
                    downloadTask.Value = p;
                });

                using var client = new HttpClient();

                return await FileDownloader.DownloadAsync(client, url, progress);
            });
    }
}
