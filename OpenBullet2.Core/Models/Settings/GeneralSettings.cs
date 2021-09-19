using System.Collections.Generic;

namespace OpenBullet2.Core.Models.Settings
{
    /// <summary>
    /// The available sections in which every part of a config can be edited.
    /// </summary>
    public enum ConfigSection
    {
        Metadata,
        Readme,
        Stacker,
        LoliCode,
        Settings,
        CSharpCode
    }

    /// <summary>
    /// The level of detail when displaying information about a job.
    /// </summary>
    public enum JobDisplayMode
    {
        Standard = 0,
        Detailed = 1
    }

    /// <summary>
    /// A target to be used as proxy check.
    /// </summary>
    public class ProxyCheckTarget
    {
        /// <summary>
        /// The URL of the website that the proxy will send a GET query to.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// A keyword that must be present in the HTTP response body in order
        /// to mark the proxy as working. Case sensitive.
        /// </summary>
        public string SuccessKey { get; set; }

        public ProxyCheckTarget(string url = "https://google.com", string successKey = "title>Google")
        {
            Url = url;
            SuccessKey = successKey;
        }

        public override string ToString() => $"{Url} | {SuccessKey}";
    }

    /// <summary>
    /// A custom LoliCode snippet for editor autocompletion.
    /// </summary>
    public class CustomSnippet
    {
        /// <summary>
        /// The name of the snippet which will need to be typed (at least partially) to get the suggestion.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The body of the snippet which will be inserted by the editor.
        /// </summary>
        public string Body { get; set; } = "The body of your snippet";

        /// <summary>
        /// The description of what the snippet does.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// General settings of OpenBullet 2.
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Which page to navigate to on config load.
        /// </summary>
        public ConfigSection ConfigSectionOnLoad { get; set; } = ConfigSection.Stacker;

        /// <summary>
        /// Whether to automatically set the recommended amount of bots specified by a config
        /// when selecting a config in a job.
        /// </summary>
        public bool AutoSetRecommendedBots { get; set; } = true;

        /// <summary>
        /// Whether to output a warning upon quitting or loading a new config when
        /// the previous one was edited but not saved.
        /// </summary>
        public bool WarnConfigNotSaved { get; set; } = true;

        /// <summary>
        /// The default author to use when creating new configs.
        /// </summary>
        public string DefaultAuthor { get; set; } = "Anonymous";

        /// <summary>
        /// Whether to display the job log in the interface.
        /// </summary>
        public bool EnableJobLogging { get; set; } = false;

        /// <summary>
        /// The maximum amount of log entries that are saved in memory for each job.
        /// </summary>
        public int LogBufferSize { get; set; } = 30;

        /// <summary>
        /// Whether to ignore the wordlist name when removing duplicate hits (so that similar hits
        /// obtained using different wordlists are treated as duplicate).
        /// </summary>
        public bool IgnoreWordlistNameOnHitsDedupe { get; set; } = false;

        /// <summary>
        /// The available targets that can be used to check proxies.
        /// </summary>
        public List<ProxyCheckTarget> ProxyCheckTargets { get; set; }

        /// <summary>
        /// The default display mode for job information.
        /// </summary>
        public JobDisplayMode DefaultJobDisplayMode { get; set; } = JobDisplayMode.Standard;

        /// <summary>
        /// The refresh interval for periodically displaying a job's progress and information
        /// (in milliseconds).
        /// </summary>
        public int JobUpdateInterval { get; set; } = 1000;

        /// <summary>
        /// The refresh interval for periodically displaying all jobs' progress and information
        /// in the job manager page (in milliseconds).
        /// </summary>
        public int JobManagerUpdateInterval { get; set; } = 1000;

        /// <summary>
        /// Whether to group captured variables together in the variables log of the debugger.
        /// </summary>
        public bool GroupCapturesInDebugger { get; set; } = false;

        /// <summary>
        /// The localization culture for the UI.
        /// </summary>
        public string Culture { get; set; } = "en";

        /// <summary>
        /// Custom user-defined snippets for editor autocompletion.
        /// </summary>
        public List<CustomSnippet> CustomSnippets { get; set; } = new();
    }
}
