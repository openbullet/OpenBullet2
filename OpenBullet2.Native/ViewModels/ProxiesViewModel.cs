using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using RuriLib.Models.Proxies;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Native.ViewModels
{
    public class ProxiesViewModel : ViewModelBase
    {
        private ObservableCollection<ProxyGroupEntity> proxyGroupsCollection;
        private ObservableCollection<ProxyEntity> proxiesCollection;
        private readonly IProxyGroupRepository proxyGroupRepo;
        private readonly IProxyRepository proxyRepo;
        private bool initialized = false;
        private ProxyGroupEntity selectedGroup;

        public ObservableCollection<ProxyEntity> ProxiesCollection
        {
            get => proxiesCollection;
            private set
            {
                proxiesCollection = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProxyGroupEntity> ProxyGroupsCollection
        {
            get => proxyGroupsCollection;
            private set
            {
                proxyGroupsCollection = value;
                OnPropertyChanged();
            }
        }

        public ProxyGroupEntity SelectedGroup
        {
            get => selectedGroup;
            private set
            {
                selectedGroup = value;
                OnPropertyChanged();
            }
        }

        public int Total => ProxiesCollection.Count;

        public ProxiesViewModel()
        {
            proxyGroupRepo = SP.GetService<IProxyGroupRepository>();
            proxyRepo = SP.GetService<IProxyRepository>();
            ProxiesCollection = new ObservableCollection<ProxyEntity>();
        }

        public async Task Initialize()
        {
            if (!initialized)
            {
                await RefreshList();
                initialized = true;
            }
        }

        public async Task RefreshList()
        {
            var items = selectedGroup == null
                ? await proxyRepo.GetAll().ToListAsync()
                : await proxyRepo.GetAll().Include(p => p.Group).Where(p => p.Group.Id == selectedGroup.Id).ToListAsync();

            ProxiesCollection = new ObservableCollection<ProxyEntity>(items);
        }

        public async Task Delete(ProxyEntity proxy)
        {
            ProxiesCollection.Remove(proxy);
            await proxyRepo.Delete(proxy);
        }

        public async Task DeleteGroup()
        {
            ProxyGroupsCollection.Remove(selectedGroup);
            await proxyGroupRepo.Delete(selectedGroup);
            SelectedGroup = null;
            await RefreshList();
        }

        public async Task DeleteNotWorking()
        {
            var toRemove = proxiesCollection.Where(p => p.Status == ProxyWorkingStatus.NotWorking);
            await proxyRepo.Delete(toRemove);
            await RefreshList();
        }
    }
}
