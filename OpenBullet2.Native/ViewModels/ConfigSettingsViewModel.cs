using OpenBullet2.Core.Services;
using RuriLib.Models.Configs;
using RuriLib.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenBullet2.Native.ViewModels
{
    public class ConfigSettingsViewModel : ViewModelBase
    {
        private readonly RuriLibSettingsService rlSettingsService;
        private readonly ConfigService configService;
        private Config Config => configService.SelectedConfig;

        private string continueStatuses;
        public string ContinueStatuses
        {
            get => continueStatuses;
            set
            {
                continueStatuses = value;
                Config.Settings.GeneralSettings.ContinueStatuses = 
                    continueStatuses.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
                OnPropertyChanged();
            }
        }

        public ConfigSettingsViewModel()
        {
            configService = SP.GetService<ConfigService>();
            rlSettingsService = SP.GetService<RuriLibSettingsService>();
        }

        public override void UpdateViewModel()
        {
            CreateCollections();
            base.UpdateViewModel();
        }

        private void CreateCollections()
        {
            ContinueStatuses = string.Join(',', Config.Settings.GeneralSettings.ContinueStatuses);
        }
    }
}
