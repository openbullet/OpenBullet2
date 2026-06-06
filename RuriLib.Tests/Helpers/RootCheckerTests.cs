using System.Runtime.InteropServices;
using RuriLib.Helpers;
using Xunit;

namespace RuriLib.Tests.Helpers;

public class RootCheckerTests
{
    [Fact]
    public void IsUnixRoot_OnWindows_ReturnsFalse()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        Assert.False(RootChecker.IsUnixRoot());
    }

    [Fact]
    public void IsRoot_OnUnix_MatchesIsUnixRoot()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        Assert.Equal(RootChecker.IsUnixRoot(), RootChecker.IsRoot());
    }
}
