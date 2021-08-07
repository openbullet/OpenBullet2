using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenBullet2.Native.ViewModels
{
    public class OBSettingsViewModel : ViewModelBase
    {
        private OpenBulletSettingsService service;
        private GeneralSettings General => service.Settings.GeneralSettings;
        private RemoteSettings Remote => service.Settings.RemoteSettings;
        private SecuritySettings Security => service.Settings.SecuritySettings;
        private CustomizationSettings Customization => service.Settings.CustomizationSettings;

        public OBSettingsViewModel()
        {
            service = SP.GetService<OpenBulletSettingsService>();
        }

        public ConfigSection ConfigSectionOnLoad
        {
            get => General.ConfigSectionOnLoad;
            set
            {
                General.ConfigSectionOnLoad = value;
                OnPropertyChanged();
            }
        }

        public bool AutoSetRecommendedBots
        {
            get => General.AutoSetRecommendedBots;
            set
            {
                General.AutoSetRecommendedBots = value;
                OnPropertyChanged();
            }
        }

        public bool WarnConfigNotSaved
        {
            get => General.WarnConfigNotSaved;
            set
            {
                General.WarnConfigNotSaved = value;
                OnPropertyChanged();
            }
        }

        public string DefaultAuthor
        {
            get => General.DefaultAuthor;
            set
            {
                General.DefaultAuthor = value;
                OnPropertyChanged();
            }
        }

        public Task Save() => service.Save();

        public void Reset()
        {
            service.Recreate();
            
            // Call OnPropertyChanged on all public properties
            foreach (var property in GetType().GetProperties())
            {
                OnPropertyChanged(property.Name);
            }
        }
    }
}
