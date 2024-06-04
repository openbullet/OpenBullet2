using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace OpenBullet2.Native.Updater.Helpers;

public static class FileSystemHelper
{
    private static readonly string[] _whitelist = new[]
    {
        "appsettings.json",
        "UserData"
    };
    
    public static async Task<Version?> GetLocalVersionAsync()
    {
        return await AnsiConsole.Status()
            .StartAsync("[yellow]Reading the current version...[/]", async ctx => 
            {
                // Check if version.txt exists
                if (!File.Exists("version.txt"))
                {
                    return null;
                }
                        
                var content = await File.ReadAllLinesAsync("version.txt");
                var currentVersion = Version.Parse(content.First());
                        
                AnsiConsole.MarkupLineInterpolated($"[green]Current version: {currentVersion}[/]");
                    
                return currentVersion;
            });
    }

    public static async Task CleanupInstallationFolderAsync()
    {
        AnsiConsole.MarkupLine("[yellow]Cleaning up the OB2 folder...[/]");
            
        

        // The build-files.txt file contains a list of all the files in the current build.
        // We will delete all those files and folders and clean up the directory.
        if (File.Exists("build-files.txt"))
        {
            var entries = await File.ReadAllLinesAsync("build-files.txt");
            
            AnsiConsole.Status()
                .Start("[yellow]Deleting...[/]", ctx =>
                {
                    foreach (var entry in entries.Where(e => !string.IsNullOrWhiteSpace(e)))
                    {
                        ctx.Status($"Deleting {entry}...");
                
                        // If the entry is whitelisted, disregard it
                        if (_whitelist.Contains(entry))
                        {
                            continue;
                        }
                        
                        // If it's the current executable, disregard it
                        if (entry == Process.GetCurrentProcess().MainModule?.FileName)
                        {
                            continue;
                        }
                        
                        var path = Path.Combine(Directory.GetCurrentDirectory(), entry);
                        
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        else if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                    }
                });
                
        }
        // If the file does not exist, we will delete everything except the whitelisted files and folders.
        else
        {
            AnsiConsole.MarkupLine(
                "[yellow]build-files.txt not found, cleaning up everything not in the whitelist...[/]");
            
            AnsiConsole.Status()
                .Start("[yellow]Deleting...[/]", ctx =>
                {
                    foreach (var entry in Directory.EnumerateFileSystemEntries(Directory.GetCurrentDirectory()))
                    {
                        var isDirectory = (File.GetAttributes(entry) & FileAttributes.Directory) == FileAttributes.Directory;
                    
                        // If the entry is whitelisted, disregard it
                        if (_whitelist.Contains(Path.GetFileName(entry)))
                        {
                            continue;
                        }
                        
                        // If it's the current executable, disregard it
                        if (entry == Process.GetCurrentProcess().MainModule?.FileName)
                        {
                            continue;
                        }
                    
                        ctx.Status($"Deleting {entry}...");
                
                        if (isDirectory)
                        {
                            Directory.Delete(entry, true);
                        }
                        else
                        {
                            File.Delete(entry);
                        }
                    }
                });
        }
    }

    public static async Task ExtractArchiveAsync(Stream stream)
    {
        AnsiConsole.MarkupLine("[yellow]Extracting the archive...[/]");
            
            await AnsiConsole.Status()
                .StartAsync("[yellow]Extracting...[/]", async ctx => 
                {
                    using var archive = new ZipArchive(stream);
                    foreach (var entry in archive.Entries)
                    {
                        // If the entry is whitelisted, disregard it
                        if (_whitelist.Contains(Path.GetFileName(entry.FullName)))
                        {
                            continue;
                        }
                
                        // Do not extract anything in the UserData folder (important)
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
    }
}
