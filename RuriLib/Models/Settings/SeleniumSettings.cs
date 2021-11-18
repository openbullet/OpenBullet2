namespace RuriLib.Models.Settings
{
    public class SeleniumSettings
    {
        public SeleniumBrowserType BrowserType { get; set; } = SeleniumBrowserType.Chrome;
        public string ChromeBinaryLocation { get; set; } = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
        public string FirefoxBinaryLocation { get; set; } = @"C:\Program Files\Mozilla Firefox\firefox.exe";
        // public bool DrawMouseMovement { get; set; } = true;
    }

    public enum SeleniumBrowserType
    {
        Chrome,
        Firefox
    }
}
