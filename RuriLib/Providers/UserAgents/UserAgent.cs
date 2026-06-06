namespace RuriLib.Providers.UserAgents;

/// <summary>
/// Represents a weighted User-Agent entry.
/// </summary>
public struct UserAgent
{
    /// <summary>
    /// The platform family.
    /// </summary>
    public readonly UAPlatform platform;
    /// <summary>
    /// The User-Agent string.
    /// </summary>
    public readonly string userAgentString;
    /// <summary>
    /// The selection weight.
    /// </summary>
    public readonly double weight;
    /// <summary>
    /// The cumulative selection weight.
    /// </summary>
    public readonly double cumulative;

    /// <summary>
    /// Creates a weighted User-Agent entry.
    /// </summary>
    public UserAgent(string userAgentString, UAPlatform platform, double weight, double cumulative)
    {
        this.userAgentString = userAgentString;
        this.weight = weight;
        this.cumulative = cumulative;
        this.platform = platform;
    }
}
