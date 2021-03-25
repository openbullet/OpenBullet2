using RuriLib.Providers.Security;
using System.Security.Cryptography.X509Certificates;

namespace RuriLib.Tests.Utils.Mockup
{
    public class MockedSecurityProvider : ISecurityProvider
    {
        public bool RestrictBlocksToCWD => false;

        public X509RevocationMode X509RevocationMode { get; set; } = X509RevocationMode.NoCheck;
    }
}
