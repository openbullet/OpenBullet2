using OpenBullet2.Models.Configs;
using RuriLib.Models.Environment;
using System;
using System.Collections.Generic;

namespace OpenBullet2
{
    // Just for testing
    public static class Static
    {
        public static List<Config> Configs { get; set; }
        public static event EventHandler<Config> ConfigSelected;

        private static Config config = null;
        public static Config Config
        {
            get => config;
            set
            {
                config = value;
                ConfigSelected.Invoke(null, config);
            }
        }

        public static EnvironmentSettings Environment { get; set; } =
            EnvironmentSettings.FromIni("Environment.ini");
    }
}
