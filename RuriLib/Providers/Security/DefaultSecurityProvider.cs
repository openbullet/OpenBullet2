using RuriLib.Services;

namespace RuriLib.Providers.Security
{
    public class DefaultSecurityProvider : ISecurityProvider
    {
        public bool RestrictBlocksToCWD { get; }

        public DefaultSecurityProvider(RuriLibSettingsService settings)
        {
            RestrictBlocksToCWD = settings.RuriLibSettings.GeneralSettings.RestrictBlocksToCWD;
        }
    }
}
