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

    public enum JobDisplayMode
    {
        Standard = 0,
        Detailed = 1
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
        public int LogBufferSize { get; set; } = 30;
        public bool IgnoreWordlistNameOnHitsDedupe { get; set; } = false;
        public List<ProxyCheckTarget> ProxyCheckTargets { get; set; }
        public JobDisplayMode DefaultJobDisplayMode { get; set; } = JobDisplayMode.Standard;
        public int JobUpdateInterval { get; set; } = 1000;
        public int JobManagerUpdateInterval { get; set; } = 1000;
        public bool GroupCapturesInDebugger { get; set; } = false;
        public string Culture { get; set; } = "en";
    }
}
