using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

const string Version = "v1.5.6";
const string Repository = "lexiforest/curl-impersonate";

var requestedRid = GetOption(args, "--rid") ?? "current";
var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
var outputRoot = Path.Combine(repoRoot, "RuriLib.Http", "runtimes");
var assets = AssetCatalog.All.ToDictionary(a => a.Rid, StringComparer.OrdinalIgnoreCase);

var rids = requestedRid.Equals("all", StringComparison.OrdinalIgnoreCase)
    ? AssetCatalog.All.Select(a => a.Rid).ToArray()
    : [requestedRid.Equals("current", StringComparison.OrdinalIgnoreCase) ? GetCurrentRid() : requestedRid];

using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("OpenBullet2", "curl-impersonate-assets"));

foreach (var rid in rids)
{
    if (!assets.TryGetValue(rid, out var asset))
    {
        throw new InvalidOperationException($"Unsupported RID '{rid}'. Supported RIDs: {string.Join(", ", assets.Keys)}");
    }

    Console.WriteLine($"Fetching curl-impersonate {Version} for {rid}");
    var archive = await DownloadAndVerifyAsync(http, asset);
    ExtractNativeFiles(asset, archive, outputRoot);
}

return;

static string? GetOption(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }

    return null;
}

static string FindRepoRoot(string start)
{
    var directory = new DirectoryInfo(start);

    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "RuriLib.Http", "RuriLib.Http.csproj")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException("Could not locate the OpenBullet2 repository root.");
}

static string GetCurrentRid()
{
    var arch = RuntimeInformation.OSArchitecture switch
    {
        Architecture.X64 => "x64",
        Architecture.Arm64 => "arm64",
        _ => throw new NotSupportedException($"Unsupported architecture: {RuntimeInformation.OSArchitecture}")
    };

    if (OperatingSystem.IsWindows())
    {
        return $"win-{arch}";
    }

    if (OperatingSystem.IsMacOS())
    {
        return $"osx-{arch}";
    }

    if (OperatingSystem.IsLinux())
    {
        return $"linux-{arch}";
    }

    throw new NotSupportedException($"Unsupported OS: {RuntimeInformation.OSDescription}");
}

static async Task<string> DownloadAndVerifyAsync(HttpClient http, NativeAsset asset)
{
    var cacheDir = Path.Combine(Path.GetTempPath(), "openbullet2-curl-impersonate", Version);
    Directory.CreateDirectory(cacheDir);

    var archivePath = Path.Combine(cacheDir, asset.FileName);

    if (!File.Exists(archivePath) || !HashMatches(archivePath, asset.Sha256))
    {
        var url = $"https://github.com/{Repository}/releases/download/{Version}/{asset.FileName}";
        await using var remote = await http.GetStreamAsync(url);
        await using var local = File.Create(archivePath);
        await remote.CopyToAsync(local);
    }

    if (!HashMatches(archivePath, asset.Sha256))
    {
        throw new InvalidOperationException($"SHA-256 mismatch for {asset.FileName}");
    }

    return archivePath;
}

static bool HashMatches(string path, string expectedSha256)
{
    using var stream = File.OpenRead(path);
    var hash = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    return hash.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase);
}

static void ExtractNativeFiles(NativeAsset asset, string archivePath, string outputRoot)
{
    var ridNativeDir = Path.Combine(outputRoot, asset.Rid, "native");

    if (Directory.Exists(ridNativeDir))
    {
        Directory.Delete(ridNativeDir, recursive: true);
    }

    Directory.CreateDirectory(ridNativeDir);

    var copied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var links = new List<(string LinkName, string TargetName)>();

    using (var archive = File.OpenRead(archivePath))
    using (var gzip = new GZipStream(archive, CompressionMode.Decompress))
    using (var reader = new TarReader(gzip))
    {
        TarEntry? entry;

        while ((entry = reader.GetNextEntry()) is not null)
        {
            if (!ShouldCopy(asset.Rid, entry.Name))
            {
                continue;
            }

            var fileName = Path.GetFileName(entry.Name);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }

            switch (entry.EntryType)
            {
                case TarEntryType.RegularFile:
                case TarEntryType.V7RegularFile:
                    if (entry.DataStream is null)
                    {
                        throw new InvalidOperationException($"Native file '{entry.Name}' has no data stream.");
                    }

                    using (var output = File.Create(Path.Combine(ridNativeDir, fileName)))
                    {
                        entry.DataStream.CopyTo(output);
                    }

                    copied.Add(fileName);
                    Console.WriteLine($"  {asset.Rid}/native/{fileName}");
                    break;

                case TarEntryType.SymbolicLink:
                case TarEntryType.HardLink:
                    if (!string.IsNullOrWhiteSpace(entry.LinkName))
                    {
                        links.Add((fileName, Path.GetFileName(entry.LinkName)));
                    }

                    break;
            }
        }
    }

    foreach (var (linkName, targetName) in links)
    {
        if (copied.Contains(linkName) || string.IsNullOrWhiteSpace(targetName))
        {
            continue;
        }

        var targetPath = Path.Combine(ridNativeDir, targetName);

        if (!File.Exists(targetPath))
        {
            continue;
        }

        File.Copy(targetPath, Path.Combine(ridNativeDir, linkName), overwrite: true);
        copied.Add(linkName);
        Console.WriteLine($"  {asset.Rid}/native/{linkName}");
    }

    if (copied.Count == 0)
    {
        throw new InvalidOperationException($"No native files found in {asset.FileName}");
    }
}

static bool ShouldCopy(string rid, string path)
{
    var fileName = Path.GetFileName(path);

    if (rid.StartsWith("win-", StringComparison.OrdinalIgnoreCase))
    {
        return fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
    }

    if (rid.StartsWith("osx-", StringComparison.OrdinalIgnoreCase))
    {
        return fileName.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase);
    }

    return fileName.Equals("libcurl-impersonate.so", StringComparison.OrdinalIgnoreCase)
        || fileName.StartsWith("libcurl-impersonate.so.", StringComparison.OrdinalIgnoreCase);
}

internal sealed record NativeAsset(string Rid, string FileName, string Sha256);

internal static class AssetCatalog
{
    public static readonly NativeAsset[] All =
    [
        new("win-x64", "libcurl-impersonate-v1.5.6.x86_64-win32.tar.gz", "fe8ce2488d5467fda6061b8b130b5834bc30cdfff40712692e8c5685dbbda6c7"),
        new("win-arm64", "libcurl-impersonate-v1.5.6.arm64-win32.tar.gz", "bf8e04e5162b1cf13ec5bac94b97f11fad3eca1125ecb73bdfe142cbe65d2590"),
        new("osx-x64", "libcurl-impersonate-v1.5.6.x86_64-macos.tar.gz", "05589344cac1ef5aaee89397c2070e45f12eeeba4f0cfba79780a28c46d8a751"),
        new("osx-arm64", "libcurl-impersonate-v1.5.6.arm64-macos.tar.gz", "00f89f687d9940d13642af90fd192d976897ffd35828e3d859d3fd02cf7fb31f"),
        new("linux-x64", "libcurl-impersonate-v1.5.6.x86_64-linux-gnu.tar.gz", "f07e25084020c54d6fd5654c8d458e09b3a44c312f88e480c255399f00487b25"),
        new("linux-arm64", "libcurl-impersonate-v1.5.6.aarch64-linux-gnu.tar.gz", "b4e4f713655616efd2be83153d9057b5961c15e34563dde09a8b6798a8b331e9")
    ];
}
