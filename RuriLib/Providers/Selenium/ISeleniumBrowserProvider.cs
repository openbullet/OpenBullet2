using RuriLib.Models.Settings;

namespace RuriLib.Providers.Selenium
{
    public interface ISeleniumBrowserProvider
    {
        string ChromeBinaryLocation { get; }
        string FirefoxBinaryLocation { get; }
        SeleniumBrowserType BrowserType { get; }
    }
}
