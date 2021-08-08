using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
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
