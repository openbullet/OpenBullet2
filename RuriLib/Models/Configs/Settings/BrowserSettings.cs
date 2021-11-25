using System;
using System.Collections.Generic;

namespace RuriLib.Models.Configs.Settings
{
    public class BrowserSettings
    {
        public string[] QuitBrowserStatuses { get; set; } = Array.Empty<string>();
        public bool Headless { get; set; } = true;
        public string CommandLineArgs { get; set; } = "--disable-notifications";
        public bool IgnoreHttpsErrors { get; set; } = false;
        public bool LoadOnlyDocumentAndScript { get; set; } = false;
        public bool DismissDialogs { get; set; } = false;
        public List<string> BlockedUrls { get; set; } = new();
    }
}
