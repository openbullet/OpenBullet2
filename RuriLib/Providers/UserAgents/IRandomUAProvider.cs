namespace RuriLib.Providers.UserAgents
{
    public interface IRandomUAProvider
    {
        int Total { get; }
        string Generate();
        string Generate(UAPlatform platform);
    }
}
