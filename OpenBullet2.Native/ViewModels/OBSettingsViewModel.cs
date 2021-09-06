using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Native.ViewModels
{
    public class OBSettingsViewModel : ViewModelBase
    {
        private OpenBulletSettingsService service;
        private GeneralSettings General => service.Settings.GeneralSettings;
        private RemoteSettings Remote => service.Settings.RemoteSettings;
        private CustomizationSettings Customization => service.Settings.CustomizationSettings;

        public OBSettingsViewModel()
        {
            service = SP.GetService<OpenBulletSettingsService>();
            CreateCollections();
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

        public bool EnableJobLogging
        {
            get => General.EnableJobLogging;
            set
            {
                General.EnableJobLogging = value;
                OnPropertyChanged();
            }
        }

        public int LogBufferSize
        {
            get => General.LogBufferSize;
            set
            {
                General.LogBufferSize = value;
                OnPropertyChanged();
            }
        }

        public JobDisplayMode DefaultJobDisplayMode
        {
            get => General.DefaultJobDisplayMode;
            set
            {
                General.DefaultJobDisplayMode = value;
                OnPropertyChanged();
            }
        }

        public bool GroupCapturesInDebugger
        {
            get => General.GroupCapturesInDebugger;
            set
            {
                General.GroupCapturesInDebugger = value;
                OnPropertyChanged();
            }
        }

        public bool IgnoreWordlistNameOnHitsDedupe
        {
            get => General.IgnoreWordlistNameOnHitsDedupe;
            set
            {
                General.IgnoreWordlistNameOnHitsDedupe = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ProxyCheckTarget> proxyCheckTargetsCollection;
        public ObservableCollection<ProxyCheckTarget> ProxyCheckTargetsCollection
        {
            get => proxyCheckTargetsCollection;
            set
            {
                proxyCheckTargetsCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RemoteConfigsEndpoint> remoteConfigsEndointsCollection;
        public ObservableCollection<RemoteConfigsEndpoint> RemoteConfigsEndpointsCollection
        {
            get => remoteConfigsEndointsCollection;
            set
            {
                remoteConfigsEndointsCollection = value;
                OnPropertyChanged();
            }
        }

        public bool PlaySoundOnHit
        {
            get => Customization.PlaySoundOnHit;
            set
            {
                Customization.PlaySoundOnHit = value;
                OnPropertyChanged();
            }
        }

        public string BackgroundMain
        {
            get => Customization.BackgroundMain;
            set
            {
                Customization.BackgroundMain = value;
                Brush.SetAppColor("BackgroundMain", value);
                OnPropertyChanged();
            }
        }

        public string BackgroundSecondary
        {
            get => Customization.BackgroundSecondary;
            set
            {
                Customization.BackgroundSecondary = value;
                Brush.SetAppColor("BackgroundSecondary", value);
                OnPropertyChanged();
            }
        }

        public string BackgroundInput
        {
            get => Customization.BackgroundInput;
            set
            {
                Customization.BackgroundInput = value;
                Brush.SetAppColor("BackgroundInput", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundMain
        {
            get => Customization.ForegroundMain;
            set
            {
                Customization.ForegroundMain = value;
                Brush.SetAppColor("ForegroundMain", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundInput
        {
            get => Customization.ForegroundInput;
            set
            {
                Customization.ForegroundInput = value;
                Brush.SetAppColor("ForegroundInput", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundGood
        {
            get => Customization.ForegroundGood;
            set
            {
                Customization.ForegroundGood = value;
                Brush.SetAppColor("ForegroundGood", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundBad
        {
            get => Customization.ForegroundBad;
            set
            {
                Customization.ForegroundBad = value;
                Brush.SetAppColor("ForegroundBad", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundCustom
        {
            get => Customization.ForegroundCustom;
            set
            {
                Customization.ForegroundCustom = value;
                Brush.SetAppColor("ForegroundCustom", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundRetry
        {
            get => Customization.ForegroundRetry;
            set
            {
                Customization.ForegroundRetry = value;
                Brush.SetAppColor("ForegroundRetry", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundBanned
        {
            get => Customization.ForegroundBanned;
            set
            {
                Customization.ForegroundBanned = value;
                Brush.SetAppColor("ForegroundBanned", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundToCheck
        {
            get => Customization.ForegroundToCheck;
            set
            {
                Customization.ForegroundToCheck = value;
                Brush.SetAppColor("ForegroundToCheck", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundMenuSelected
        {
            get => Customization.ForegroundMenuSelected;
            set
            {
                Customization.ForegroundMenuSelected = value;
                Brush.SetAppColor("ForegroundMenuSelected", value);
                OnPropertyChanged();
            }
        }

        public string SuccessButton
        {
            get => Customization.SuccessButton;
            set
            {
                Customization.SuccessButton = value;
                Brush.SetAppColor("SuccessButton", value);
                OnPropertyChanged();
            }
        }

        public string PrimaryButton
        {
            get => Customization.PrimaryButton;
            set
            {
                Customization.PrimaryButton = value;
                Brush.SetAppColor("PrimaryButton", value);
                OnPropertyChanged();
            }
        }

        public string WarningButton
        {
            get => Customization.WarningButton;
            set
            {
                Customization.WarningButton = value;
                Brush.SetAppColor("WarningButton", value);
                OnPropertyChanged();
            }
        }

        public string DangerButton
        {
            get => Customization.DangerButton;
            set
            {
                Customization.DangerButton = value;
                Brush.SetAppColor("DangerButton", value);
                OnPropertyChanged();
            }
        }

        public string ForegroundButton
        {
            get => Customization.ForegroundButton;
            set
            {
                Customization.ForegroundButton = value;
                Brush.SetAppColor("ForegroundButton", value);
                OnPropertyChanged();
            }
        }

        public string BackgroundButton
        {
            get => Customization.BackgroundButton;
            set
            {
                Customization.BackgroundButton = value;
                Brush.SetAppColor("BackgroundButton", value);
                OnPropertyChanged();
            }
        }

        public async Task Save()
        {
            General.ProxyCheckTargets = ProxyCheckTargetsCollection.ToList();
            Remote.ConfigsEndpoints = RemoteConfigsEndpointsCollection.ToList();
            await service.Save();
        }

        public void Reset()
        {
            service.Recreate();

            CreateCollections();

            // Call OnPropertyChanged on all public properties
            foreach (var property in GetType().GetProperties())
            {
                OnPropertyChanged(property.Name);
            }

            // Reset the theme colors
            Brush.SetAppColor("BackgroundMain", Customization.BackgroundMain);
            Brush.SetAppColor("BackgroundSecondary", Customization.BackgroundSecondary);
            Brush.SetAppColor("BackgroundInput", Customization.BackgroundInput);
            Brush.SetAppColor("ForegroundMain", Customization.ForegroundMain);
            Brush.SetAppColor("ForegroundInput", Customization.ForegroundInput);
            Brush.SetAppColor("ForegroundGood", Customization.ForegroundGood);
            Brush.SetAppColor("ForegroundBad", Customization.ForegroundBad);
            Brush.SetAppColor("ForegroundCustom", Customization.ForegroundCustom);
            Brush.SetAppColor("ForegroundRetry", Customization.ForegroundRetry);
            Brush.SetAppColor("ForegroundBanned", Customization.ForegroundBanned);
            Brush.SetAppColor("ForegroundToCheck", Customization.ForegroundToCheck);
            Brush.SetAppColor("ForegroundMenuSelected", Customization.ForegroundMenuSelected);
            Brush.SetAppColor("SuccessButton", Customization.SuccessButton);
            Brush.SetAppColor("PrimaryButton", Customization.PrimaryButton);
            Brush.SetAppColor("WarningButton", Customization.WarningButton);
            Brush.SetAppColor("DangerButton", Customization.DangerButton);
            Brush.SetAppColor("ForegroundButton", Customization.ForegroundButton);
            Brush.SetAppColor("BackgroundButton", Customization.BackgroundButton);
        }

        public void AddProxyCheckTarget() => ProxyCheckTargetsCollection.Add(new ProxyCheckTarget());
        public void RemoveProxyCheckTarget(ProxyCheckTarget target) => ProxyCheckTargetsCollection.Remove(target);

        public void AddRemoteConfigsEndpoint() => RemoteConfigsEndpointsCollection.Add(new RemoteConfigsEndpoint());
        public void RemoveRemoteConfigsEndpoint(RemoteConfigsEndpoint endpoint) => RemoteConfigsEndpointsCollection.Remove(endpoint);

        private void CreateCollections()
        {
            ProxyCheckTargetsCollection = new ObservableCollection<ProxyCheckTarget>(General.ProxyCheckTargets);
            RemoteConfigsEndpointsCollection = new ObservableCollection<RemoteConfigsEndpoint>(Remote.ConfigsEndpoints);
        }
    }
}
