using System.Security.Cryptography.X509Certificates;

namespace RuriLib.Providers.Security
{
    public interface ISecurityProvider
    {
        bool RestrictBlocksToCWD { get; }
        X509RevocationMode X509RevocationMode { get; set; }
    }
}
