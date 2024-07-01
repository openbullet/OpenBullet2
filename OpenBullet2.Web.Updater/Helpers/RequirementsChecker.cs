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

namespace OpenBullet2.Web.Updater.Helpers;

public static class RequirementsChecker
{
    private static readonly Version _dotnetVersion = new(8, 0);
    
    public static async Task EnsureOb2WebNotRunningAsync()
    {
        var isRunning = false;
        
        await AnsiConsole.Status()
            .StartAsync("[yellow]Checking if OpenBullet 2 is running...[/]", async ctx =>
            {
                // Wait for 10 seconds for the process to close
                var timeout = TimeSpan.FromSeconds(10);
                while (Process.GetProcessesByName("OpenBullet2.Web").Length > 0 && timeout > TimeSpan.Zero)
                {
                    ctx.Status("[yellow]OpenBullet 2 is running, waiting for it to close...[/]")
                        .Spinner(Spinner.Known.Dots)
                        .Refresh();
                    
                    await Task.Delay(1000);
                    timeout -= TimeSpan.FromSeconds(1);
                }
                
                if (Process.GetProcessesByName("OpenBullet2.Web").Length > 0)
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
    /// Checks if the ASP.NET Core Runtime is installed. If the user installed the SDK,
    /// it will still work because the runtime is included in the SDK.
    /// </summary>
    public static async Task EnsureDotNetInstalledAsync()
    {
        if (await IsRuntimeInstalledAsync())
        {
            return;
        }

        var installRuntime = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            && AnsiConsole.Prompt(
                new ConfirmationPrompt($"The .NET Runtime and ASP.NET Core Runtime version {_dotnetVersion} or higher are required to run OpenBullet 2. " +
                                       "Do you want to download and install them now?"));
        
        if (!installRuntime)
        {
            Utils.ExitWithError($"The .NET Runtime and ASP.NET Core Runtime version {_dotnetVersion} or higher are required to run OpenBullet 2. " +
                                $"Please install them from https://dotnet.microsoft.com/en-us/download/dotnet/{_dotnetVersion} " +
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
            // Microsoft.AspNetCore.App 8.0.0 [/usr/local/share/dotnet/shared/Microsoft.AspNetCore.App]
            // Microsoft.NETCore.App 8.0.0 [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
            return result.ExitCode == 0 && result.StandardOutput.Split('\n').Any(line =>
                line.StartsWith($"Microsoft.AspNetCore.App {_dotnetVersion.Major}."));
        }
        catch
        {
            return false;
        }
    }
    
    // We need to install both the .NET Runtime and the ASP.NET Core Runtime
    private static async Task InstallDotNetRuntimeAsync()
    {
        // Download and install the .NET Runtime
        var runtimeFileName = RuntimeInformation.OSArchitecture switch
        {
            Architecture.Arm64 => "dotnet-runtime-win-arm64.exe",
            Architecture.X64 => "dotnet-runtime-win-x64.exe",
            Architecture.X86 => "dotnet-runtime-win-x86.exe",
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
        
        // Download and install the ASP.NET Core Runtime
        runtimeFileName = RuntimeInformation.OSArchitecture switch
        {
            Architecture.Arm64 => "aspnetcore-runtime-win-arm64.exe",
            Architecture.X64 => "aspnetcore-runtime-win-x64.exe",
            Architecture.X86 => "aspnetcore-runtime-win-x86.exe",
            _ => throw new NotImplementedException()
        };

        downloadUrl = $"https://aka.ms/dotnet/{_dotnetVersion}/{runtimeFileName}";
        
        await using var aspNetCoreRuntimeStream = await DownloadRuntimeAsync(downloadUrl);

        tempPath = Path.GetTempFileName() + ".exe";
        await using (var tempStream = File.OpenWrite(tempPath))
        {
            await aspNetCoreRuntimeStream.CopyToAsync(tempStream);
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
