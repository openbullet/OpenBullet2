using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Spectre.Console;

namespace OpenBullet2.Native.Updater.Helpers;

public static class RequirementsChecker
{
    private static readonly Version _dotnetVersion = new(8, 0);
    
    public static async Task EnsureOb2NativeNotRunningAsync()
    {
        var isRunning = false;
        
        await AnsiConsole.Status()
            .StartAsync("[yellow]Checking if OpenBullet 2 is running...[/]", async ctx =>
            {
                // Wait for 10 seconds for the process to close
                var timeout = TimeSpan.FromSeconds(10);
                while (Process.GetProcessesByName("OpenBullet2.Native").Length > 0 && timeout > TimeSpan.Zero)
                {
                    ctx.Status("[yellow]OpenBullet 2 is running, waiting for it to close...[/]")
                        .Spinner(Spinner.Known.Dots)
                        .Refresh();
                    
                    await Task.Delay(1000);
                    timeout -= TimeSpan.FromSeconds(1);
                }
                
                if (Process.GetProcessesByName("OpenBullet2.Native").Length > 0)
                {
                    isRunning = true;
                }
            });

        if (isRunning)
        {
            Utils.ExitWithError("OpenBullet 2 is currently running, please close it before updating!");
        }
    }

    /// <summary>
    /// Checks if the .NET Windows Desktop Runtime is installed. If the user installed the SDK,
    /// it will still work because the runtime is included in the SDK.
    /// </summary>
    public static async Task EnsureDotNetInstalledAsync()
    {
        if (await IsRuntimeInstalledAsync())
        {
            return;
        }
        
        var installRuntime = AnsiConsole.Prompt(
            new ConfirmationPrompt($"The .NET Windows Desktop Runtime version {_dotnetVersion} or higher is required to run OpenBullet 2. " +
                                   "Do you want to download and install it now?"));

        if (!installRuntime)
        {
            Utils.ExitWithError($"The .NET Windows Desktop Runtime version {_dotnetVersion} or higher is required to run OpenBullet 2. " +
                                $"Please install it from https://dotnet.microsoft.com/en-us/download/dotnet/{_dotnetVersion} " +
                                "and relaunch the Updater");
        }
        
        await InstallDotNetRuntimeAsync();
    }

    private static async Task<bool> IsRuntimeInstalledAsync()
    {
        try
        {
            // If dotnet is not a valid command, this will throw an exception
            var result = await Cli.Wrap("dotnet")
                .WithArguments("--list-runtimes")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
            
            // The output of the command is something like:
            // Microsoft.WindowsDesktop.App 8.0.0 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            return result.ExitCode == 0 && result.StandardOutput.Split('\n').Any(line =>
                line.StartsWith($"Microsoft.WindowsDesktop.App {_dotnetVersion.Major}."));
        }
        catch
        {
            return false;
        }
    }
    
    // The .NET Windows Desktop Runtime also includes the .NET Runtime
    private static async Task InstallDotNetRuntimeAsync()
    {
        var runtimeFileName = RuntimeInformation.OSArchitecture switch
        {
            Architecture.Arm64 => "windowsdesktop-runtime-win-arm64.exe",
            Architecture.X64 => "windowsdesktop-runtime-win-x64.exe",
            Architecture.X86 => "windowsdesktop-runtime-win-x86.exe",
            _ => throw new NotImplementedException()
        };

        var downloadUrl = $"https://aka.ms/dotnet/{_dotnetVersion}/{runtimeFileName}";
        
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
