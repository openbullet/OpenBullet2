using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;

namespace OpenBullet2.Services
{
    public class ConfigService
    {
        public List<Config> Configs { get; set; }
        public event EventHandler<Config> OnConfigSelected;

        private Config selectedConfig = null;
        public Config SelectedConfig
        {
            get => selectedConfig;
            set
            {
                selectedConfig = value;
                OnConfigSelected.Invoke(null, selectedConfig);
            }
        }
    }
}
