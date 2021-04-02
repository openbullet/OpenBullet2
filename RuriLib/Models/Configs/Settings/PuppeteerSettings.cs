using System;

namespace RuriLib.Models.Configs.Settings
{
    public class PuppeteerSettings
    {
        public string[] QuitBrowserStatuses { get; set; } = Array.Empty<string>();
        public string CommandLineArgs { get; set; } = "--disable-notifications";
        public bool Headless { get; set; } = true;
        public bool IgnoreHttpsErrors { get; set; } = false;
        public bool LoadDocument { get; set; } = true;
        public bool LoadStylesheet { get; set; } = true;
        public bool LoadImage { get; set; } = true;
        public bool LoadMedia { get; set; } = true;
        public bool LoadFont { get; set; } = true;
        public bool LoadScript { get; set; } = true;
        public bool LoadTexttrack { get; set; } = true;
        public bool LoadXhr { get; set; } = true;
        public bool LoadFetch { get; set; } = true;
        public bool LoadEventsource { get; set; } = true;
        public bool LoadWebsocket { get; set; } = true;
        public bool LoadManifest { get; set; } = true;
        public bool LoadOther { get; set; } = true;
    }
}