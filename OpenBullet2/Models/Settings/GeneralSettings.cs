using System.Collections.Generic;

namespace OpenBullet2.Models.Settings
{
    public enum ConfigSection
    {
        Metadata,
        Readme,
        Stacker,
        LoliCode,
        Settings,
        CSharpCode
    }

    public class ProxyCheckTarget
    {
        public string Url { get; set; } = "https://google.com";
        public string SuccessKey { get; set; } = "title>Google";
    }

    public class GeneralSettings
    {
        /// <summary>Which page to navigate to on config load.</summary>
        public ConfigSection ConfigSectionOnLoad { get; set; } = ConfigSection.Stacker;
        public bool AutoSetRecommendedBots { get; set; } = true;
        public bool WarnConfigNotSaved { get; set; } = true;
        public string DefaultAuthor { get; set; } = "Anonymous";
        public bool EnableJobLogging { get; set; } = false;
        public int LogBufferSize { get; set; } = 20;
        public bool LogToFile { get; set; } = false;
        public bool IgnoreWordlistNameOnHitsDedupe { get; set; } = false;
        public List<ProxyCheckTarget> ProxyCheckTargets { get; set; }
    }
}
