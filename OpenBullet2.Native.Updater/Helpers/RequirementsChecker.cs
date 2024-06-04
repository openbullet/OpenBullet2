using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace OpenBullet2.Native.Updater.Helpers;

public static class RequirementsChecker
{
    public static void EnsureOb2NativeNotRunning()
    {
        if (Process.GetProcessesByName("OpenBullet2.Native").Length > 0)
        {
            Utils.ExitWithError("OpenBullet 2 is currently running, please close it before updating!");
        }
    }

    /// <summary>
    /// Checks if the .NET Windows Desktop Runtime 8.0+ is installed. If the user installed the SDK,
    /// it will still work because the runtime is included in the SDK.
    /// </summary>
    public static async Task EnsureDotNetInstalledAsync()
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
            
            if (result.ExitCode == 0)
            {
                if (!result.StandardOutput.Split('\n').Any(line => line.StartsWith("Microsoft.WindowsDesktop.App 8.")))
                {
                    Utils.ExitWithError("The .NET Windows Desktop Runtime version 8.0 or higher is required to run OpenBullet 2. " +
                                        "Please install it from https://dotnet.microsoft.com/en-us/download/dotnet/8.0 " +
                                        "and relaunch the Updater");
                }
            }
            else
            {
                Utils.ExitWithError("The .NET Windows Desktop Runtime version 8.0 or higher was not found on your system, " +
                                    "and is required to run OpenBullet 2. Please install it from " +
                                    "https://dotnet.microsoft.com/en-us/download/dotnet/8.0 and relaunch the Updater.");
            }
        }
        catch (Exception)
        {
            Utils.ExitWithError("The .NET Windows Desktop Runtime version 8.0 or higher was not found on your system, " +
                                "and is required to run OpenBullet 2. Please install it from " +
                                "https://dotnet.microsoft.com/en-us/download/dotnet/8.0 and relaunch the Updater.");
        }
    }
}
