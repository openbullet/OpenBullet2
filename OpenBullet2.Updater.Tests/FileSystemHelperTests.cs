using System.IO.Compression;
using Xunit;

namespace OpenBullet2.Updater.Tests;

public class FileSystemHelperTests
{
    [Fact]
    public async Task ApplyUpdateAsync_ReplacesAddsAndRemovesBuildFiles()
    {
        using var install = new TemporaryDirectory();
        WriteInstallFile(install.Path, "version.txt", "0.1.0");
        WriteInstallFile(install.Path, "build-files.txt", string.Join(Environment.NewLine,
            "version.txt",
            "build-files.txt",
            "old.txt",
            "removed.txt",
            "nested"));
        WriteInstallFile(install.Path, "old.txt", "old content");
        WriteInstallFile(install.Path, "removed.txt", "remove me");
        WriteInstallFile(install.Path, Path.Combine("nested", "old-nested.txt"), "old nested");

        await using var archive = CreateArchive(
            ("version.txt", "0.2.0"),
            ("build-files.txt", string.Join(Environment.NewLine,
                "version.txt",
                "build-files.txt",
                "old.txt",
                "added.txt",
                "nested")),
            ("old.txt", "new content"),
            ("added.txt", "added content"),
            (Path.Combine("nested", "new-nested.txt"), "new nested"));

        await ApplyUpdateAsync(archive, install.Path);

        Assert.Equal("0.2.0", ReadInstallFile(install.Path, "version.txt"));
        Assert.Equal("new content", ReadInstallFile(install.Path, "old.txt"));
        Assert.Equal("added content", ReadInstallFile(install.Path, "added.txt"));
        Assert.Equal("new nested", ReadInstallFile(install.Path, Path.Combine("nested", "new-nested.txt")));
        Assert.False(File.Exists(Path.Combine(install.Path, "removed.txt")));
        Assert.False(File.Exists(Path.Combine(install.Path, "nested", "old-nested.txt")));
        Assert.Empty(Directory.GetDirectories(install.Path, ".ob2-update-*"));
    }

    [Fact]
    public async Task ApplyUpdateAsync_PreservesAppsettingsAndUserData()
    {
        using var install = new TemporaryDirectory();
        WriteInstallFile(install.Path, "version.txt", "0.1.0");
        WriteInstallFile(install.Path, "build-files.txt", string.Join(Environment.NewLine,
            "version.txt",
            "build-files.txt",
            "appsettings.json",
            "UserData",
            "old.txt"));
        WriteInstallFile(install.Path, "appsettings.json", """{"preserved":true}""");
        WriteInstallFile(install.Path, Path.Combine("UserData", "Configs", "config.opk"), "user config");
        WriteInstallFile(install.Path, "old.txt", "old content");

        await using var archive = CreateArchive(
            ("version.txt", "0.2.0"),
            ("build-files.txt", string.Join(Environment.NewLine,
                "version.txt",
                "build-files.txt",
                "appsettings.json",
                "UserData",
                "old.txt")),
            ("appsettings.json", """{"fromArchive":true}"""),
            (Path.Combine("UserData", "Configs", "config.opk"), "archive config"),
            ("old.txt", "new content"));

        await ApplyUpdateAsync(archive, install.Path);

        Assert.Equal("""{"preserved":true}""", ReadInstallFile(install.Path, "appsettings.json"));
        Assert.Equal("user config", ReadInstallFile(install.Path, Path.Combine("UserData", "Configs", "config.opk")));
        Assert.Equal("new content", ReadInstallFile(install.Path, "old.txt"));
    }

    [Fact]
    public async Task ApplyUpdateAsync_CleanInstall_WithTopLevelDirectory_Succeeds()
    {
        using var install = new TemporaryDirectory();

        await using var archive = CreateArchive(
            ("version.txt", "0.3.3.2960"),
            ("build-files.txt", string.Join(Environment.NewLine,
                "version.txt",
                "build-files.txt",
                "libraries",
                "OpenBullet2.Web.dll")),
            ("OpenBullet2.Web.dll", "web assembly"),
            (Path.Combine("libraries", "dependency.dll"), "dependency"));

        await ApplyUpdateAsync(archive, install.Path);

        Assert.Equal("0.3.3.2960", ReadInstallFile(install.Path, "version.txt"));
        Assert.Equal("web assembly", ReadInstallFile(install.Path, "OpenBullet2.Web.dll"));
        Assert.Equal("dependency", ReadInstallFile(install.Path, Path.Combine("libraries", "dependency.dll")));
        Assert.Empty(Directory.GetDirectories(install.Path, ".ob2-update-*"));
    }

    [Fact]
    public async Task ApplyUpdateAsync_RestoresPreviousBuildWhenStagedPayloadIsInvalid()
    {
        using var install = new TemporaryDirectory();
        WriteInstallFile(install.Path, "version.txt", "0.1.0");
        WriteInstallFile(install.Path, "build-files.txt", string.Join(Environment.NewLine,
            "version.txt",
            "build-files.txt",
            "old.txt",
            "appsettings.json",
            "UserData"));
        WriteInstallFile(install.Path, "old.txt", "old content");
        WriteInstallFile(install.Path, "appsettings.json", """{"preserved":true}""");
        WriteInstallFile(install.Path, Path.Combine("UserData", "data.txt"), "user data");

        await using var archive = CreateArchive(
            ("version.txt", "0.2.0"),
            ("old.txt", "new content"));

        await Assert.ThrowsAsync<InvalidDataException>(() => ApplyUpdateAsync(archive, install.Path));

        Assert.Equal("0.1.0", ReadInstallFile(install.Path, "version.txt"));
        Assert.Equal("old content", ReadInstallFile(install.Path, "old.txt"));
        Assert.Equal("""{"preserved":true}""", ReadInstallFile(install.Path, "appsettings.json"));
        Assert.Equal("user data", ReadInstallFile(install.Path, Path.Combine("UserData", "data.txt")));
        Assert.Empty(Directory.GetDirectories(install.Path, ".ob2-update-*"));
    }

    [Fact]
    public async Task ApplyUpdateAsync_RejectsArchiveEntriesOutsideInstallDirectory()
    {
        using var install = new TemporaryDirectory();
        WriteInstallFile(install.Path, "version.txt", "0.1.0");
        WriteInstallFile(install.Path, "build-files.txt", string.Join(Environment.NewLine,
            "version.txt",
            "build-files.txt",
            "old.txt"));
        WriteInstallFile(install.Path, "old.txt", "old content");

        await using var archive = CreateArchive(
            ("version.txt", "0.2.0"),
            ("build-files.txt", string.Join(Environment.NewLine, "version.txt", "build-files.txt", "old.txt")),
            ("../outside.txt", "outside"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => ApplyUpdateAsync(archive, install.Path));

        Assert.Equal("0.1.0", ReadInstallFile(install.Path, "version.txt"));
        Assert.Equal("old content", ReadInstallFile(install.Path, "old.txt"));
        Assert.False(File.Exists(Path.Combine(install.Path, "..", "outside.txt")));
        Assert.Empty(Directory.GetDirectories(install.Path, ".ob2-update-*"));
    }

    [Fact]
    public async Task ApplyUpdateAsync_RejectsBuildFileEntriesOutsideInstallDirectory()
    {
        using var install = new TemporaryDirectory();
        WriteInstallFile(install.Path, "version.txt", "0.1.0");
        WriteInstallFile(install.Path, "build-files.txt", string.Join(Environment.NewLine,
            "version.txt",
            "build-files.txt",
            "old.txt"));
        WriteInstallFile(install.Path, "old.txt", "old content");

        await using var archive = CreateArchive(
            ("version.txt", "0.2.0"),
            ("build-files.txt", string.Join(Environment.NewLine,
                "version.txt",
                "build-files.txt",
                "../outside.txt",
                "old.txt")),
            ("old.txt", "new content"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => ApplyUpdateAsync(archive, install.Path));

        Assert.Equal("0.1.0", ReadInstallFile(install.Path, "version.txt"));
        Assert.Equal("old content", ReadInstallFile(install.Path, "old.txt"));
        Assert.Empty(Directory.GetDirectories(install.Path, ".ob2-update-*"));
    }

    private static MemoryStream CreateArchive(params (string Path, string Content)[] entries)
    {
        var stream = new MemoryStream();

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (path, content) in entries)
            {
                var entry = archive.CreateEntry(path.Replace('\\', '/'));
                using var writer = new StreamWriter(entry.Open());
                writer.Write(content);
            }
        }

        stream.Position = 0;
        return stream;
    }

    private static void WriteInstallFile(string installDirectory, string relativePath, string content)
    {
        var path = Path.Combine(installDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static string ReadInstallFile(string installDirectory, string relativePath)
        => File.ReadAllText(Path.Combine(installDirectory, relativePath));

    private static Task ApplyUpdateAsync(Stream stream, string installDirectory)
        => OpenBullet2.Updater.Core.Helpers.FileSystemHelper.ApplyUpdateAsync(stream, installDirectory);

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"ob2-updater-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
