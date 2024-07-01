using CommandLine;
using Spectre.Console;
using System;
using System.Threading.Tasks;
using OpenBullet2.Web.Updater.Helpers;

namespace OpenBullet2.Web.Updater;

public static class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            await new Parser(with => { with.CaseInsensitiveEnumValues = true; }).ParseArguments<CliOptions>(args)
                .WithParsedAsync(async opts => await UpdateAsync(opts));
        }
        catch (Exception ex)
        {
            Utils.ExitWithError(ex);
        }
    }

    private static async Task UpdateAsync(CliOptions options)
    {
        // Validate the repository
        InputValidation.ValidateRepository(options.Repository);
        
        // If the channel was not specified, ask the user
        options.Channel ??= Channel.AskForChannel();
        
        // Check if OpenBullet2 is running
        await RequirementsChecker.EnsureOb2WebNotRunningAsync();
        
        // Make sure the user has the required .NET runtime installed
        await RequirementsChecker.EnsureDotNetInstalledAsync();

        // Fetch info from remote
        using var githubClient = new GitHubClient(options.Repository, options.Channel.Value, options.Username, options.Token);
        var remoteVersionInfo = await githubClient.FetchRemoteVersionAsync();

        AnsiConsole.MarkupLineInterpolated($"[green]Remote version: {remoteVersionInfo.Version}[/]");
        
        // Get the current version
        var currentVersion = await FileSystemHelper.GetLocalVersionAsync();

        if (currentVersion is null)
        {
            // If the current version is null, assume it's a clean install
            AnsiConsole.MarkupLine("[yellow]version.txt not found, assuming this is a clean install[/]");
                        
            // Ask the user if they want to proceed and download the latest version
            var cleanInstall = AnsiConsole.Prompt(
                new ConfirmationPrompt("Do you want to proceed and download the latest version?"));
                        
            // If the user said no, exit
            if (!cleanInstall)
            {
                AnsiConsole.MarkupLine("[yellow]Exiting...[/]");
                Environment.Exit(0);
            }
        }
        else
        {
            if (remoteVersionInfo.Version > currentVersion)
            {
                AnsiConsole.MarkupLine("[yellow]Update available![/]");
                
                // Ask the user if they want to proceed and update to the latest version
                var update = AnsiConsole.Prompt(
                    new ConfirmationPrompt("Do you want to proceed and update to the latest version?"));
                
                // If the user said no, exit
                if (!update)
                {
                    AnsiConsole.MarkupLine("[yellow]Exiting...[/]");
                    Environment.Exit(0);
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[green]Already up to date![/]");
                AnsiConsole.MarkupLine("[green]Press any key to exit...[/]");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
        
        // Download the new build
        await using var buildStream = await githubClient.DownloadBuildAsync(remoteVersionInfo);
        AnsiConsole.MarkupLine("[green]Download complete![/]");

        // Clean up the installation folder
        await FileSystemHelper.CleanupInstallationFolderAsync();

        // Extract the archive
        await FileSystemHelper.ExtractArchiveAsync(buildStream);
        
        AnsiConsole.MarkupLine("[green]The update was completed successfully. " +
                               "You may now restart your OpenBullet 2 instance![/]");
        AnsiConsole.MarkupLine("[green]Press any key to exit...[/]");
        Console.ReadKey();
        Environment.Exit(0);
    }
}
