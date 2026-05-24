using System;
using System.Runtime.InteropServices;

namespace OpenBullet2.Updater.Core.Helpers;

public static class ReleaseAssetNames
{
    public static string GetCurrentWebAssetName()
        => GetWebAssetName(GetCurrentOsPlatform(), RuntimeInformation.OSArchitecture);

    public static string GetCurrentNativeAssetName()
        => GetNativeAssetName(GetCurrentOsPlatform(), RuntimeInformation.OSArchitecture);

    public static string GetWebAssetName(Architecture architecture)
        => GetWebAssetName(GetCurrentOsPlatform(), architecture);

    public static string GetNativeAssetName(Architecture architecture)
        => GetNativeAssetName(GetCurrentOsPlatform(), architecture);

    public static string GetWebAssetName(OSPlatform osPlatform, Architecture architecture)
        => osPlatform == OSPlatform.Windows
            ? architecture switch
            {
                Architecture.X64 => "OpenBullet2.Web-win-x64.zip",
                Architecture.Arm64 => "OpenBullet2.Web-win-arm64.zip",
                _ => throw new NotSupportedException($"Unsupported Windows architecture {architecture}")
            }
            : osPlatform == OSPlatform.OSX
                ? architecture switch
                {
                    Architecture.X64 => "OpenBullet2.Web-osx-x64.zip",
                    Architecture.Arm64 => "OpenBullet2.Web-osx-arm64.zip",
                    _ => throw new NotSupportedException($"Unsupported macOS architecture {architecture}")
                }
            : osPlatform == OSPlatform.Linux
                ? architecture switch
                {
                    Architecture.X64 => "OpenBullet2.Web-linux-x64.zip",
                    Architecture.Arm64 => "OpenBullet2.Web-linux-arm64.zip",
                    _ => throw new NotSupportedException($"Unsupported Linux architecture {architecture}")
                }
                : throw new NotSupportedException("Only Windows, macOS and Linux are supported by the web updater");

    public static string GetNativeAssetName(OSPlatform osPlatform, Architecture architecture)
        => osPlatform == OSPlatform.Windows
            ? architecture switch
            {
                Architecture.X64 => "OpenBullet2.Native-win-x64.zip",
                Architecture.Arm64 => "OpenBullet2.Native-win-arm64.zip",
                _ => throw new NotSupportedException($"Unsupported Windows architecture {architecture}")
            }
            : throw new NotSupportedException("Only Windows is supported by the native updater");

    private static OSPlatform GetCurrentOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OSPlatform.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return OSPlatform.Linux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OSPlatform.OSX;
        }

        throw new NotSupportedException("Only Windows, macOS and Linux are supported by the web updater");
    }
}
