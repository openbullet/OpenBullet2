using System.Runtime.InteropServices;
using OpenBullet2.Updater.Core.Helpers;
using Xunit;

namespace OpenBullet2.Updater.Tests;

public class ReleaseAssetNamesTests
{
    [Theory]
    [InlineData(Architecture.X64, "OpenBullet2.Web-win-x64.zip")]
    [InlineData(Architecture.X86, "OpenBullet2.Web-win-x64.zip")]
    [InlineData(Architecture.Arm64, "OpenBullet2.Web-win-x64.zip")]
    public void GetWebAssetName_ReturnsWindowsAssetForAllWindowsArchitectures(Architecture architecture, string expected)
        => Assert.Equal(expected, ReleaseAssetNames.GetWebAssetName(OSPlatform.Windows, architecture));

    [Theory]
    [InlineData(Architecture.X64, "OpenBullet2.Web-linux-x64.zip")]
    [InlineData(Architecture.Arm64, "OpenBullet2.Web-linux-arm64.zip")]
    public void GetWebAssetName_ReturnsLinuxAssetForSupportedArchitectures(Architecture architecture, string expected)
        => Assert.Equal(expected, ReleaseAssetNames.GetWebAssetName(OSPlatform.Linux, architecture));

    [Theory]
    [InlineData(Architecture.X86)]
    [InlineData(Architecture.Arm)]
    public void GetWebAssetName_ThrowsForUnsupportedLinuxArchitectures(Architecture architecture)
        => Assert.Throws<NotSupportedException>(() => ReleaseAssetNames.GetWebAssetName(OSPlatform.Linux, architecture));
}
