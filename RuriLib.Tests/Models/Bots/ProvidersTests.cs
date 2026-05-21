using RuriLib.Models.Bots;
using Xunit;

namespace RuriLib.Tests.Models.Bots;

public class ProvidersTests
{
    [Fact]
    public void Constructor_WithNullSettings_StillInitializesRng()
    {
        var providers = new global::RuriLib.Models.Bots.Providers(null);

        Assert.NotNull(providers.RNG);
        Assert.Null(providers.RandomUA);
        Assert.Null(providers.Captcha);
        Assert.Null(providers.EmailDomains);
        Assert.NotNull(providers.BrowserAutomation);
        Assert.Null(providers.PuppeteerBrowser);
        Assert.Null(providers.SeleniumBrowser);
        Assert.Null(providers.GeneralSettings);
        Assert.Null(providers.ProxySettings);
        Assert.Null(providers.Security);
    }
}
