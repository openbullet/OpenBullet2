using RuriLib.Parallelization;
using System.Collections.Generic;

namespace RuriLib.Models.Settings;

/// <summary>
/// Stores general runtime settings.
/// </summary>
public class GeneralSettings
{
    /// <summary>Gets or sets the parallelizer implementation to use.</summary>
    public ParallelizerType ParallelizerType { get; set; } = ParallelizerType.TaskBased;
    /// <summary>Gets or sets a value indicating whether job activity should be written to a file.</summary>
    public bool LogJobActivityToFile { get; set; }
    /// <summary>Gets or sets a value indicating whether blocks are restricted to the current working directory.</summary>
    public bool RestrictBlocksToCWD { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether a custom User-Agent list is enabled.</summary>
    public bool UseCustomUserAgentsList { get; set; }
    /// <summary>Gets or sets a value indicating whether bot logging is enabled.</summary>
    public bool EnableBotLogging { get; set; }
    /// <summary>Gets or sets a value indicating whether verbose mode is enabled.</summary>
    public bool VerboseMode { get; set; }
    /// <summary>Gets or sets a value indicating whether all results should be logged.</summary>
    public bool LogAllResults { get; set; }
    /// <summary>Gets or sets the list of proxy judge URLs used to determine proxy anonymity.</summary>
    public List<string> ProxyJudgeUrls { get; set; } =
    [
        "http://azenv.net/",
        "http://proxyjudge.us/",
        "http://httpheader.net/azenv.php"
    ];
    /// <summary>Gets or sets the configured User-Agent list.</summary>
    public List<string> UserAgents { get; set; } = [];
}
