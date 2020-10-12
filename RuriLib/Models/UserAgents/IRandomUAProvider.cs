using RuriLib.Models.UserAgents;

namespace RuriLib.Models.UserAgents
{
    public interface IRandomUAProvider
    {
        int Total { get; }
        string Generate();
        string Generate(UAPlatform platform);
    }
}
