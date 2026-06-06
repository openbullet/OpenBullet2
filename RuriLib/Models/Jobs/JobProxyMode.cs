namespace RuriLib.Models.Jobs;

/// <summary>
/// Controls how a job should use proxies.
/// </summary>
public enum JobProxyMode
{
    /// <summary>Always use proxies.</summary>
    On,
    /// <summary>Never use proxies.</summary>
    Off,
    /// <summary>Use the config default.</summary>
    Default
}
