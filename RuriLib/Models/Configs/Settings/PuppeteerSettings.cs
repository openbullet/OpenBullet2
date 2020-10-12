namespace RuriLib.Models.Configs.Settings
{
    public class PuppeteerSettings
    {
        public bool AlwaysOpenBrowser { get; set; } = false;
        public string[] QuitBrowserStatuses { get; set; } = new string[] { };
        public bool Headless { get; set; } = true;
        public string CommandLineArgs { get; set; } = "--disable-notifications";
        public bool IgnoreHttpsErrors { get; set; } = false;
    }
}
