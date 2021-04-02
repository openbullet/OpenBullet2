using System;

namespace RuriLib.Models.Configs.Settings
{
    public class PuppeteerSettings
    {
        public string[] QuitBrowserStatuses { get; set; } = Array.Empty<string>();
        public bool Headless { get; set; } = true;
        public string CommandLineArgs { get; set; } = "--disable-notifications";
        public bool IgnoreHttpsErrors { get; set; } = false;
    }
}
