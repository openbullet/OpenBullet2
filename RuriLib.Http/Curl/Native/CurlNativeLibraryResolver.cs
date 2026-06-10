using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RuriLib.Http.Curl.Native;

internal static class CurlNativeLibraryResolver
{
    private static readonly object Lock = new();
    private static bool initialized;

    public static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        lock (Lock)
        {
            if (initialized)
            {
                return;
            }

            // Register once for this assembly so DllImport("curl-impersonate")
            // works even though the native file names differ by OS.
            NativeLibrary.SetDllImportResolver(typeof(CurlNativeLibraryResolver).Assembly, ResolveLibrary);
            initialized = true;
        }
    }

    private static nint ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!libraryName.Equals(CurlNativeMethods.CurlLibrary, StringComparison.Ordinal)
            && !libraryName.Equals(CurlNativeMethods.CurlShimLibrary, StringComparison.Ordinal))
        {
            return 0;
        }

        var platformName = GetPlatformLibraryName(libraryName);

        foreach (var candidate in GetCandidatePaths(platformName))
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out var handle))
            {
                return handle;
            }
        }

        if (NativeLibrary.TryLoad(platformName, assembly, searchPath, out var resolvedHandle))
        {
            return resolvedHandle;
        }

        if (NativeLibrary.TryLoad(platformName, out resolvedHandle))
        {
            return resolvedHandle;
        }

        throw new DllNotFoundException(
            $"Unable to load curl-impersonate native library '{platformName}'. " +
            $"Run 'dotnet run --project Tools/CurlImpersonate.NativeAssets -- --rid {GetRid()}' " +
            "or provide the native library in the application directory or system library path.");
    }

    private static string GetPlatformLibraryName(string libraryName)
    {
        var isShim = libraryName.Equals(CurlNativeMethods.CurlShimLibrary, StringComparison.Ordinal);
        var baseName = isShim ? "libcurl-impersonate-shim" : "libcurl-impersonate";

        if (OperatingSystem.IsWindows())
        {
            return $"{baseName}.dll";
        }

        if (OperatingSystem.IsMacOS())
        {
            return $"{baseName}.dylib";
        }

        return $"{baseName}.so";
    }

    private static string GetRid()
    {
        var arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            _ => RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()
        };

        if (OperatingSystem.IsWindows())
        {
            return $"win-{arch}";
        }

        if (OperatingSystem.IsMacOS())
        {
            return $"osx-{arch}";
        }

        return $"linux-{arch}";
    }

    private static string[] GetCandidatePaths(string libraryName)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var rid = GetRid();

        // Published apps place assets under runtimes/<rid>/native. Test runs and
        // local dev builds may execute from bin/, so also walk upward to find the
        // downloaded source-tree assets under RuriLib.Http/runtimes/<rid>/native.
        var candidates = new List<string>
        {
            Path.Combine(baseDirectory, "runtimes", rid, "native", libraryName),
            Path.Combine(baseDirectory, libraryName),
            Path.Combine(baseDirectory, "..", "..", "runtimes", rid, "native", libraryName),
            Path.Combine(baseDirectory, "..", "..", "..", "runtimes", rid, "native", libraryName)
        };

        foreach (var parent in EnumerateParents(baseDirectory))
        {
            candidates.Add(Path.Combine(parent, "RuriLib.Http", "runtimes", rid, "native", libraryName));
        }

        return [.. candidates];
    }

    private static IEnumerable<string> EnumerateParents(string start)
    {
        var directory = new DirectoryInfo(start);

        while (directory is not null)
        {
            yield return directory.FullName;
            directory = directory.Parent;
        }
    }
}
