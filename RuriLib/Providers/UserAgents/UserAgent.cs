namespace RuriLib.Providers.UserAgents
{
    public struct UserAgent
    {
        public readonly UAPlatform platform;
        public readonly string userAgentString;
        public readonly double weight;
        public readonly double cumulative;

        public UserAgent(string userAgentString, UAPlatform platform, double weight, double cumulative)
        {
            this.userAgentString = userAgentString;
            this.weight = weight;
            this.cumulative = cumulative;
            this.platform = platform;
        }
    }
}
