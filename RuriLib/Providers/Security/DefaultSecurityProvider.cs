using RuriLib.Services;
using System.Security.Cryptography.X509Certificates;

namespace RuriLib.Providers.Security
{
    public class DefaultSecurityProvider : ISecurityProvider
    {
        public bool RestrictBlocksToCWD { get; }
        public X509RevocationMode X509RevocationMode { get; set; } = X509RevocationMode.NoCheck;

        public DefaultSecurityProvider(RuriLibSettingsService settings)
        {
            RestrictBlocksToCWD = settings.RuriLibSettings.GeneralSettings.RestrictBlocksToCWD;
        }
    }
}
