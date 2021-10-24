namespace RuriLib.Models.Settings
{
    public class GlobalSettings
    {
        public GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
        public CaptchaSettings CaptchaSettings { get; set; } = new CaptchaSettings();
        public ProxySettings ProxySettings { get; set; } = new ProxySettings();
        public PuppeteerSettings PuppeteerSettings { get; set; } = new PuppeteerSettings();
        public SeleniumSettings SeleniumSettings { get; set; } = new SeleniumSettings();
    }
}
