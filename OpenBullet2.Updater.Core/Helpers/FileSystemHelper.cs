using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.Versioning;
using Spectre.Console;

namespace OpenBullet2.Updater.Core.Helpers;

public static class FileSystemHelper
{
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    public static string ResolveInstallDirectory(string? installDirectory)
    {
        var path = string.IsNullOrWhiteSpace(installDirectory)
            ? AppContext.BaseDirectory
            : installDirectory;

        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(fullPath);

        return fullPath;
    }

    private static string GetSafeInstallationPath(string installDirectory, string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException($"Unsafe absolute path: {relativePath}");
        }

        var root = Path.GetFullPath(installDirectory);
        var rootWithSeparator = Path.EndsInDirectorySeparator(root)
            ? root
            : root + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(root, relativePath));

        if (!path.StartsWith(rootWithSeparator, PathComparison))
        {
            throw new InvalidOperationException($"Unsafe path outside the installation directory: {relativePath}");
        }

        return path;
    }

    public static async Task<Version?> GetLocalVersionAsync(string installDirectory)
    {
        return await AnsiConsole.Status()
            .StartAsync("[yellow]Reading the current version...[/]", async ctx =>
            {
                var versionFile = Path.Combine(installDirectory, "version.txt");

                // Check if version.txt exists
                if (!File.Exists(versionFile))
                {
                    return null;
                }

                var content = await File.ReadAllLinesAsync(versionFile);
                var currentVersion = Version.Parse(content.First());

                AnsiConsole.MarkupLineInterpolated($"[green]Current version: {currentVersion}[/]");

                return currentVersion;
            });
    }

    public static async Task ApplyUpdateAsync(Stream stream, string installDirectory)
    {
        var updateDirectory = Path.Combine(installDirectory, $".ob2-update-{Guid.NewGuid():N}");
        var stagingDirectory = Path.Combine(updateDirectory, "staging");
        var backupDirectory = Path.Combine(updateDirectory, "backup");
        string[] stagedEntries = [];
        var installStarted = false;

        try
        {
            Directory.CreateDirectory(stagingDirectory);
            Directory.CreateDirectory(backupDirectory);

            await ExtractArchiveAsync(stream, stagingDirectory, installDirectory);
            ValidateStagedBuild(stagingDirectory);

            stagedEntries = await ReadBuildFileEntriesAsync(stagingDirectory);
            await BackupCurrentBuildAsync(installDirectory, backupDirectory);

            installStarted = true;
            MoveStagedBuild(installDirectory, stagingDirectory, backupDirectory, stagedEntries);
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                EnsureUnixExecutablePermissions(installDirectory, stagedEntries);
            }

            Directory.Delete(updateDirectory, true);
        }
        catch
        {
            if (stagedEntries.Length > 0 && Directory.Exists(backupDirectory))
            {
                RestoreBackup(installDirectory, backupDirectory, stagedEntries, installStarted);
            }

            DeleteUpdateDirectory(updateDirectory);
            throw;
        }
    }

    private static async Task BackupCurrentBuildAsync(string installDirectory, string backupDirectory)
    {
        AnsiConsole.MarkupLine("[yellow]Backing up the current OB2 build...[/]");

        if (!File.Exists(Path.Combine(installDirectory, "build-files.txt")))
        {
            AnsiConsole.MarkupLine("[yellow]build-files.txt not found, skipping old build cleanup...[/]");
            return;
        }

        var entries = await ReadBuildFileEntriesAsync(installDirectory);

        AnsiConsole.Status()
            .Start("[yellow]Backing up...[/]", ctx =>
            {
                var currentExecutable = Process.GetCurrentProcess().MainModule?.FileName;

                foreach (var entry in entries)
                {
                    ctx.Status($"Backing up {entry}...");

                    if (ShouldAlwaysPreserveEntry(entry) || ShouldPreserveExistingEntry(installDirectory, entry))
                    {
                        continue;
                    }

                    var source = GetSafeInstallationPath(installDirectory, entry);

                    if (string.Equals(source, currentExecutable, PathComparison) || !PathExists(source))
                    {
                        continue;
                    }

                    var destination = GetSafeInstallationPath(backupDirectory, entry);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                    MovePath(source, destination);
                }
            });
    }

    private static void MoveStagedBuild(string installDirectory, string stagingDirectory,
        string backupDirectory, string[] stagedEntries)
    {
        AnsiConsole.MarkupLine("[yellow]Installing the new OB2 build...[/]");

        AnsiConsole.Status()
            .Start("[yellow]Installing...[/]", ctx =>
            {
                foreach (var entry in stagedEntries)
                {
                    ctx.Status($"Installing {entry}...");

                    if (ShouldAlwaysPreserveEntry(entry) || ShouldPreserveExistingEntry(installDirectory, entry))
                    {
                        continue;
                    }

                    var source = GetSafeInstallationPath(stagingDirectory, entry);

                    if (!PathExists(source))
                    {
                        continue;
                    }

                    var destination = GetSafeInstallationPath(installDirectory, entry);

                    if (PathExists(destination))
                    {
                        var backupPath = GetSafeInstallationPath(backupDirectory, entry);
                        Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                        MovePath(destination, backupPath);
                    }

                    CopyPath(source, destination);
                    TryDeletePath(source);
                }
            });
    }

    private static void RestoreBackup(string installDirectory, string backupDirectory,
        string[] stagedEntries, bool removeStagedEntries)
    {
        AnsiConsole.MarkupLine("[yellow]The update failed, restoring the previous OB2 build...[/]");

        if (removeStagedEntries)
        {
            foreach (var entry in stagedEntries)
            {
                if (ShouldAlwaysPreserveEntry(entry))
                {
                    continue;
                }

                if (TryGetSafeInstallationPath(installDirectory, entry, out var path))
                {
                    DeletePath(path);
                }
            }
        }

        foreach (var backupDir in Directory.GetDirectories(backupDirectory, "*", SearchOption.AllDirectories)
                     .OrderBy(path => path.Count(c => c == Path.DirectorySeparatorChar)))
        {
            var relativePath = Path.GetRelativePath(backupDirectory, backupDir);
            Directory.CreateDirectory(GetSafeInstallationPath(installDirectory, relativePath));
        }

        foreach (var backupFile in Directory.GetFiles(backupDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(backupDirectory, backupFile);
            var restorePath = GetSafeInstallationPath(installDirectory, relativePath);

            DeletePath(restorePath);
            Directory.CreateDirectory(Path.GetDirectoryName(restorePath)!);
            File.Move(backupFile, restorePath);
        }
    }

    private static void ValidateStagedBuild(string stagingDirectory)
    {
        if (!File.Exists(Path.Combine(stagingDirectory, "version.txt")))
        {
            throw new InvalidDataException("The update archive does not contain version.txt");
        }

        if (!File.Exists(Path.Combine(stagingDirectory, "build-files.txt")))
        {
            throw new InvalidDataException("The update archive does not contain build-files.txt");
        }
    }

    private static async Task<string[]> ReadBuildFileEntriesAsync(string directory)
        => (await File.ReadAllLinesAsync(Path.Combine(directory, "build-files.txt")))
            .Where(entry => !string.IsNullOrWhiteSpace(entry))
            .ToArray();

    private static bool ShouldAlwaysPreserveEntry(string entry)
        => entry.StartsWith("UserData");

    private static bool ShouldPreserveExistingEntry(string installDirectory, string entry)
        => entry == "appsettings.json" && File.Exists(Path.Combine(installDirectory, "appsettings.json"));

    private static bool PathExists(string path)
        => File.Exists(path) || Directory.Exists(path);

    private static bool TryGetSafeInstallationPath(string installDirectory, string relativePath, out string path)
    {
        try
        {
            path = GetSafeInstallationPath(installDirectory, relativePath);
            return true;
        }
        catch (InvalidOperationException)
        {
            path = string.Empty;
            return false;
        }
    }

    private static void MovePath(string source, string destination)
    {
        if (File.Exists(source))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Move(source, destination);
        }
        else if (Directory.Exists(source))
        {
            Directory.CreateDirectory(destination);

            foreach (var sourceDirectory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories)
                         .OrderBy(path => path.Count(c => c == Path.DirectorySeparatorChar)))
            {
                var relativePath = Path.GetRelativePath(source, sourceDirectory);
                Directory.CreateDirectory(Path.Combine(destination, relativePath));
            }

            foreach (var sourceFile in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(source, sourceFile);
                var destinationFile = Path.Combine(destination, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                if (File.Exists(destinationFile))
                {
                    File.Delete(destinationFile);
                }

                File.Move(sourceFile, destinationFile);
            }

            Directory.Delete(source, true);
        }
    }

    private static void CopyPath(string source, string destination)
    {
        if (File.Exists(source))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, overwrite: true);
        }
        else if (Directory.Exists(source))
        {
            Directory.CreateDirectory(destination);

            foreach (var sourceDirectory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories)
                         .OrderBy(path => path.Count(c => c == Path.DirectorySeparatorChar)))
            {
                var relativePath = Path.GetRelativePath(source, sourceDirectory);
                Directory.CreateDirectory(Path.Combine(destination, relativePath));
            }

            foreach (var sourceFile in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(source, sourceFile);
                var destinationFile = Path.Combine(destination, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                File.Copy(sourceFile, destinationFile, overwrite: true);
            }
        }
    }

    private static void DeletePath(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private static void DeleteUpdateDirectory(string updateDirectory)
    {
        if (Directory.Exists(updateDirectory))
        {
            Directory.Delete(updateDirectory, true);
        }
    }

    private static void TryDeletePath(string path)
    {
        try
        {
            DeletePath(path);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[yellow]Could not delete temporary path {Markup.Escape(path)} after copying, leaving it in place.[/]");
        }
    }

    private static async Task ExtractArchiveAsync(Stream stream, string targetDirectory, string installDirectory)
    {
        AnsiConsole.MarkupLine("[yellow]Extracting the archive...[/]");

        await AnsiConsole.Status()
            .StartAsync("[yellow]Extracting...[/]", async ctx =>
            {
                using var archive = new ZipArchive(stream);
                foreach (var entry in archive.Entries)
                {
                    // Do not extract appsettings.json if it exists
                    if (entry.FullName.Contains("appsettings.json") &&
                        File.Exists(Path.Combine(installDirectory, "appsettings.json")))
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

                    var path = GetSafeInstallationPath(targetDirectory, entry.FullName);
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir!);
                    }

                    await using var fileStream = new FileStream(path, FileMode.Create);
                    await using var entryStream = entry.Open();
                    await entryStream.CopyToAsync(fileStream);
                }
            });
    }

    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    private static void EnsureUnixExecutablePermissions(string installDirectory, string[] stagedEntries)
    {
        if (!(OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()))
        {
            return;
        }

        var executableMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                             UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                             UnixFileMode.OtherRead | UnixFileMode.OtherExecute;

        if (stagedEntries.Any(entry => entry.Replace('\\', '/').Equals("OpenBullet2.Web", StringComparison.Ordinal)))
        {
            TrySetUnixExecutableMode(Path.Combine(installDirectory, "OpenBullet2.Web"), executableMode);
        }

        if (stagedEntries.Any(entry => entry.Replace('\\', '/').Equals(".playwright", StringComparison.Ordinal)))
        {
            var nodeRoot = Path.Combine(installDirectory, ".playwright", "node");
            if (Directory.Exists(nodeRoot))
            {
                foreach (var nodePath in Directory.GetFiles(nodeRoot, "node", SearchOption.AllDirectories))
                {
                    TrySetUnixExecutableMode(nodePath, executableMode);
                }
            }
        }
    }

    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    private static void TrySetUnixExecutableMode(string path, UnixFileMode mode)
    {
        if (!File.Exists(path))
        {
            return;
        }

        File.SetUnixFileMode(path, mode);
    }
}
