using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace OpenBullet2.Web.Updater.Helpers;

public static class RequirementsChecker
{
    public static void EnsureOb2WebNotRunning()
    {
        if (Process.GetProcessesByName("OpenBullet2.Web").Length > 0)
        {
            Utils.ExitWithError("OpenBullet 2 is currently running, please close it before updating!");
        }
    }

    /// <summary>
    /// Checks if the ASP.NET Core Runtime 8.0+ is installed. If the user installed the SDK,
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
            // Microsoft.AspNetCore.App 8.0.0 [/usr/local/share/dotnet/shared/Microsoft.AspNetCore.App]
            // Microsoft.NETCore.App 8.0.0 [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
            
            if (result.ExitCode == 0)
            {
                if (!result.StandardOutput.Split('\n').Any(line => line.StartsWith("Microsoft.AspNetCore.App 8.")))
                {
                    Utils.ExitWithError("The ASP.NET Core Runtime version 8.0 or higher is required to run OpenBullet 2. " +
                                        "Please install it from https://dotnet.microsoft.com/en-us/download/dotnet/8.0 " +
                                        "and relaunch the Updater");
                }
            }
            else
            {
                Utils.ExitWithError("The ASP.NET Core Runtime version 8.0 or higher was not found on your system, " +
                                    "and is required to run OpenBullet 2. Please install it from " +
                                    "https://dotnet.microsoft.com/en-us/download/dotnet/8.0 and relaunch the Updater.");
            }
        }
        catch (Exception)
        {
            Utils.ExitWithError("The ASP.NET Core Runtime version 8.0 or higher was not found on your system, " +
                                "and is required to run OpenBullet 2. Please install it from " +
                                "https://dotnet.microsoft.com/en-us/download/dotnet/8.0 and relaunch the Updater.");
        }
    }
}
