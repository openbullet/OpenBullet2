using CommandLine;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Updater
{
    public class Program
    {
        private static async Task Main(string[] args) =>
            // Parse the Options
            await new Parser(with =>
                {
                    with.CaseInsensitiveEnumValues = true;
                }).ParseArguments<CliOptions>(args)
                .WithParsedAsync(async opts => await UpdateAsync(opts));

        private static async Task UpdateAsync(CliOptions options)
        {
            // Make sure the repository is in the right format
            if (!Regex.IsMatch(options.Repository, @"^[\w-]+/[\w-]+$"))
            {
                ExitWithError("The repository must be in the format owner/repo (e.g. openbullet/OpenBullet2)");
                return;
            }
            
            // If the channel was not specified, ask the user
            if (options.Channel is null)
            {
                var response = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Please select the channel")
                        .PageSize(3)
                        .AddChoices(["Staging (early builds)", "Release (stable builds)"]));
                
                options.Channel = response switch
                {
                    "Staging (early builds)" => BuildChannel.Staging,
                    "Release (stable builds)" => BuildChannel.Release,
                    _ => BuildChannel.Release
                };
            }
            
            using HttpClient client = new();
            client.BaseAddress = new Uri($"https://api.github.com/repos/{options.Repository}/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            
            AnsiConsole.MarkupLine($"[yellow]Checking for updates for {options.Repository} on the {options.Channel} channel...[/]");
            
            if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Token}")));
                AnsiConsole.MarkupLine("[yellow]Using authentication...[/]");
            }

            var release = JToken.Parse("{}");
            var remoteVersion = new Version();
            
            // Fetch info from remote
            await AnsiConsole.Status()
                .StartAsync("[yellow]Fetching version info from remote...[/]", async ctx => 
                {
                    try
                    {
                        // Query the github api to get a list of the latest releases
                        var response = await client.GetAsync("releases");

                        // Parse all the releases and versions
                        var json = await response.Content.ReadAsStringAsync();
                        var releases = JArray.Parse(json)
                            .ToDictionary(r => Version.Parse(r["tag_name"]!.ToString()), r => r);

                        // If the channel is staging, get the latest version,
                        // while if the channel is release, get the latest stable version
                        var latest = options.Channel == BuildChannel.Staging
                            ? releases.MaxBy(r => r.Key)
                            : releases.Where(r => r.Key.Revision == -1).MaxBy(r => r.Key);
                        
                        remoteVersion = latest.Key;
                        release = latest.Value;
                        
                        AnsiConsole.MarkupLine($"[green]Remote version: {remoteVersion}[/]");
                    }
                    catch (Exception ex)
                    {
                        ExitWithError(ex);
                    }
                });
            
            Version? currentVersion = null;

            await AnsiConsole.Status()
                .StartAsync("[yellow]Reading the current version...[/]", async ctx => 
                {
                    try
                    {
                        // Check if version.txt exists
                        if (!File.Exists("version.txt"))
                        {
                            return;
                        }
                        
                        var content = await File.ReadAllLinesAsync("version.txt");
                        currentVersion = Version.Parse(content.First());
                        
                        AnsiConsole.MarkupLine($"[green]Current version: {currentVersion}[/]");
                    }
                    catch (Exception ex)
                    {
                        ExitWithError(ex);
                    }
                });

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
                if (remoteVersion > currentVersion)
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
            
            // Check if OpenBullet2 is running
            if (Process.GetProcessesByName("OpenBullet2.Web").Length > 0)
            {
                ExitWithError("OpenBullet 2 is currently running, please close it before updating!");
                return;
            }
            
            // Download the OpenBullet2.Web.zip file
            MemoryStream stream = null!;
            try
            {
                var build = release["assets"]!.First(t => t["name"]!.ToObject<string>()! == "OpenBullet2.Web.zip");
                var size = build["size"]!.ToObject<double>();
                var megaBytes = size / (1 * 1000 * 1000);
                AnsiConsole.MarkupLine($"[yellow]Downloading the updated build ({megaBytes:0.00} MB)...[/]");
                
                var downloadUrl = build["url"]!.ToString();
                await AnsiConsole.Progress()
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

                        stream = await DownloadAsync(client, downloadUrl, progress); 
                    });
                
                AnsiConsole.MarkupLine("[green]Download complete![/]");
            }
            catch (Exception ex)
            {
                ExitWithError(ex);
            }
            
            AnsiConsole.MarkupLine("[yellow]Cleaning up the OB2 folder...[/]");

            // Delete all files except Updater.exe and Updater.dll
            AnsiConsole.Status()
                .Start("[yellow]Deleting...[/]", ctx =>
                {
                    foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory()))
                    {
                        if (file.EndsWith("Updater.exe") || file.EndsWith("Updater.dll"))
                        {
                            continue;
                        }
                        
                        ctx.Status($"Deleting {file}...");
                    
                        File.Delete(file);
                    }
                });
            
            // Delete all directories except UserData
            AnsiConsole.Status()
                .Start("[yellow]Deleting...[/]", ctx =>
                {
                    foreach (var dir in Directory.EnumerateDirectories(Directory.GetCurrentDirectory()))
                    {
                        if (dir.EndsWith("UserData"))
                        {
                            continue;
                        }
                        
                        ctx.Status($"Deleting {dir}...");
                    
                        Directory.Delete(dir, true);
                    }
                });
            
            AnsiConsole.MarkupLine("[yellow]Extracting the archive...[/]");
            
            await AnsiConsole.Status()
                .StartAsync("[yellow]Extracting...[/]", async ctx => 
                {
                    using var archive = new ZipArchive(stream);
                    foreach (var entry in archive.Entries)
                    {
                        // Do not extract Updater.exe or Updater.dll
                        if (entry.FullName.EndsWith("Updater.exe") || entry.FullName.EndsWith("Updater.dll"))
                        {
                            continue;
                        }
                
                        // Do not extract UserData folder
                        if (entry.FullName.StartsWith("UserData"))
                        {
                            continue;
                        }
                        
                        // If the entry is a directory, disregard it
                        if (entry.FullName.EndsWith('/'))
                        {
                            continue;
                        }
                        
                        ctx.Status($"Extracting {entry.FullName}...");
                
                        var path = Path.Combine(Directory.GetCurrentDirectory(), entry.FullName);
                        var dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir!);
                        }

                        await using var fileStream = new FileStream(path, FileMode.Create);
                        await using var entryStream = entry.Open();
                        await entryStream.CopyToAsync(fileStream);
                    }
            
                    await stream.DisposeAsync();
                });
            
            AnsiConsole.MarkupLine("[green]The update was completed successfully. " +
                                   "You may now restart your OpenBullet 2 instance![/]");
            AnsiConsole.MarkupLine("[green]Press any key to exit...[/]");
            Console.ReadKey();
            Environment.Exit(0);
        }

        private static void ExitWithError(string message)
        {
            AnsiConsole.MarkupLine($"[red]Failed! {message}[/]");
            AnsiConsole.MarkupLine("[red]Press any key to exit...[/]");
            Console.ReadKey();
            Environment.Exit(1);
        }
        
        private static void ExitWithError(Exception ex)
            => ExitWithError(ex.Message);

        private static async Task<MemoryStream> DownloadAsync(HttpClient client, string url,
            IProgress<double> progress)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // We need to specify the Accept header as application/octet-stream
            // to get the raw file instead of the json response
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var content = response.Content;
            var total = response.Content.Headers.ContentLength!;
            var downloaded = 0.0;

            var memoryStream = new MemoryStream();
            await using var stream = await content.ReadAsStreamAsync();

            var buffer = new byte[81920];
            var isMoreToRead = true;

            do
            {
                var read = await stream.ReadAsync(buffer);
                if (read == 0)
                {
                    isMoreToRead = false;
                }
                else
                {
                    await memoryStream.WriteAsync(buffer.AsMemory(0, read));
                    downloaded += read;
                    progress.Report(downloaded / total.Value * 100);
                }
            } while (isMoreToRead);
            
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
