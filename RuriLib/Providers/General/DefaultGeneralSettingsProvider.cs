using RuriLib.Models.Settings;
using RuriLib.Services;

namespace RuriLib.Providers.Proxies
{
    public class DefaultGeneralSettingsProvider : IGeneralSettingsProvider
    {
        private readonly GeneralSettings settings;

        public DefaultGeneralSettingsProvider(RuriLibSettingsService settings)
        {
            this.settings = settings.RuriLibSettings.GeneralSettings;
        }

        public bool VerboseMode => settings.VerboseMode;
        public bool LogAllResults => settings.LogAllResults;
    }
}
