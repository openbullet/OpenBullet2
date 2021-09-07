using OpenBullet2.Core.Services;
using RuriLib.Models.Configs;

namespace OpenBullet2.Native.ViewModels
{
    public class ConfigReadmeViewModel : ViewModelBase
    {
        private readonly ConfigService configService;
        private Config Config => configService.SelectedConfig;

        public string Readme
        {
            get => Config?.Readme;
            set
            {
                Config.Readme = value;
                OnPropertyChanged();
            }
        }

        public ConfigReadmeViewModel()
        {
            configService = SP.GetService<ConfigService>();
        }
    }
}
