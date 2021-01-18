using RuriLib.Models.Settings;
using RuriLib.Services;
using System;

namespace RuriLib.Providers.UserAgents
{
    public class DefaultRandomUAProvider : IRandomUAProvider
    {
        private readonly GeneralSettings settings;
        private readonly Random rand = new();

        public DefaultRandomUAProvider(RuriLibSettingsService settings)
        {
            this.settings = settings.RuriLibSettings.GeneralSettings;
        }

        public int Total => settings.UserAgents.Count;

        public string Generate()
            => settings.UserAgents[rand.Next(Total)];
        
        public string Generate(UAPlatform platform)
            => Generate();
    }
}
