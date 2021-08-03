namespace RuriLib.Providers.UserAgents
{
    /// <summary>
    /// Provides User-Agent generation capabilities.
    /// </summary>
    public interface IRandomUAProvider
    {
        /// <summary>
        /// The total number of available User-Agents.
        /// </summary>
        int Total { get; }

        /// <summary>
        /// Generates a completely random User-Agent.
        /// </summary>
        string Generate();

        /// <summary>
        /// Generates a random User-Agent for the given <paramref name="platform"/>.
        /// </summary>
        string Generate(UAPlatform platform);
    }
}
