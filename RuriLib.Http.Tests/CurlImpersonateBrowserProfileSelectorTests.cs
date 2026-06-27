using RuriLib.Http.Curl;
using RuriLib.Http.Curl.Internal;
using Xunit;

namespace RuriLib.Http.Tests;

public class CurlImpersonateBrowserProfileSelectorTests
{
    [Theory]
    [InlineData(CurlImpersonateBrowserProfile.RandomBrowser, 0,
        CurlImpersonateBrowserProfile.Chrome146)]
    [InlineData(CurlImpersonateBrowserProfile.RandomBrowser, 0.8,
        CurlImpersonateBrowserProfile.Edge101)]
    [InlineData(CurlImpersonateBrowserProfile.RandomBrowser, 0.92,
        CurlImpersonateBrowserProfile.Safari260)]
    [InlineData(CurlImpersonateBrowserProfile.RandomBrowser, 0.99,
        CurlImpersonateBrowserProfile.Firefox147)]
    [InlineData(CurlImpersonateBrowserProfile.RandomMobile, 0,
        CurlImpersonateBrowserProfile.Chrome131Android)]
    [InlineData(CurlImpersonateBrowserProfile.RandomMobile, 0.9,
        CurlImpersonateBrowserProfile.Safari260Ios)]
    [InlineData(CurlImpersonateBrowserProfile.Random, 0,
        CurlImpersonateBrowserProfile.Chrome146)]
    [InlineData(CurlImpersonateBrowserProfile.Random, 0.999,
        CurlImpersonateBrowserProfile.SafariIpad156)]
    public void Resolve_RandomProfile_SelectsExpectedWeightedEntry(
        CurlImpersonateBrowserProfile profile, double sample, CurlImpersonateBrowserProfile expected)
    {
        var resolved = CurlImpersonateBrowserProfileSelector.Resolve(profile, sample);

        Assert.Equal(expected, resolved);
    }

    [Fact]
    public void Resolve_ConcreteProfile_ReturnsItUnchanged()
    {
        var resolved = CurlImpersonateBrowserProfileSelector.Resolve(
            CurlImpersonateBrowserProfile.Firefox144, 0.5);

        Assert.Equal(CurlImpersonateBrowserProfile.Firefox144, resolved);
    }
}
