using RuriLib.Http.Curl;
using Xunit;

namespace RuriLib.Http.Tests;

public class CurlImpersonateBrowserProfileTests
{
    [Theory]
    [InlineData(CurlImpersonateBrowserProfile.Chrome150, "chrome150")]
    [InlineData(CurlImpersonateBrowserProfile.Safari2601, "safari2601")]
    public void ToCurlTarget_NewNativeProfile_ReturnsExpectedTarget(
        CurlImpersonateBrowserProfile profile, string expected)
    {
        Assert.Equal(expected, profile.ToCurlTarget());
    }

    [Theory]
    [InlineData(CurlImpersonateBrowserProfile.Chrome99Android, 17)]
    [InlineData(CurlImpersonateBrowserProfile.Random, 46)]
    [InlineData(CurlImpersonateBrowserProfile.RandomMobile, 48)]
    public void NumericValue_ExistingProfile_RemainsStable(CurlImpersonateBrowserProfile profile, int expected)
    {
        Assert.Equal(expected, (int)profile);
    }
}
