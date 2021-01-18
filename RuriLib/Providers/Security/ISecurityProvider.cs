namespace RuriLib.Providers.Security
{
    public interface ISecurityProvider
    {
        bool RestrictBlocksToCWD { get; }
    }
}
