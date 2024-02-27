using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.DTOs;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Native.ViewModels
{
    public class ProxiesViewModel : ViewModelBase
    {
        private ObservableCollection<ProxyGroupEntity> proxyGroupsCollection;
        private ObservableCollection<ProxyEntity> proxiesCollection;
        private IProxyGroupRepository proxyGroupRepo;
        private IProxyRepository proxyRepo;
        private readonly JobManagerService jobManager;
        private bool initialized;
        private ProxyGroupEntity selectedGroup;
        private readonly ProxyGroupEntity allGroup = new() { Id = -1, Name = "All" };
        
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

        public int SelectedGroupId
        {
            get => selectedGroup.Id;
            set
            {
                selectedGroup = proxyGroupsCollection.First(g => g.Id == value);
                OnPropertyChanged();
                _ = RefreshListAsync();
            }
        }

        public int Total => ProxiesCollection.Count;
        public int Working => ProxiesCollection.Count(p => p.Status == ProxyWorkingStatus.Working);
        public int NotWorking => ProxiesCollection.Count(p => p.Status == ProxyWorkingStatus.NotWorking);
        public bool GroupIsValid => selectedGroup != allGroup;
        public ProxyGroupEntity SelectedGroup => selectedGroup;

        public ProxiesViewModel()
        {
            proxyGroupRepo = SP.GetService<IProxyGroupRepository>();
            proxyRepo = SP.GetService<IProxyRepository>();
            jobManager = SP.GetService<JobManagerService>();
            ProxyGroupsCollection = new ObservableCollection<ProxyGroupEntity>
            {
                allGroup
            };
            ProxiesCollection = new ObservableCollection<ProxyEntity>();
            SelectedGroupId = allGroup.Id;
        }

        public async Task InitializeAsync()
        {
            if (!initialized)
            {
                await RefreshGroupsAsync();
                initialized = true;
            }
        }

        public async Task RefreshGroupsAsync()
        {
            SelectedGroupId = allGroup.Id;
            var entities = await proxyGroupRepo.GetAll().ToListAsync();
            ProxyGroupsCollection = new ObservableCollection<ProxyGroupEntity>(new ProxyGroupEntity[] { allGroup }.Concat(entities));

            await RefreshListAsync();
        }

        public async Task RefreshListAsync()
        {
            var items = selectedGroup == allGroup
                ? await proxyRepo.GetAll().ToListAsync()
                : await proxyRepo.GetAll().Include(p => p.Group).Where(p => p.Group.Id == selectedGroup.Id).ToListAsync();

            ProxiesCollection = new ObservableCollection<ProxyEntity>(items);
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Working));
            OnPropertyChanged(nameof(NotWorking));
        }

        public Task AddGroupAsync(ProxyGroupEntity group)
        {
            ProxyGroupsCollection.Add(group);
            SelectedGroupId = allGroup.Id;

            return proxyGroupRepo.AddAsync(group);
        }

        public async Task EditGroupAsync(ProxyGroupEntity group)
        {
            await proxyGroupRepo.UpdateAsync(group);
            await RefreshGroupsAsync();
        }

        public async Task DeleteSelectedGroupAsync()
        {
            if (selectedGroup == allGroup)
            {
                throw new Exception("Select a group first");
            }

            var firstProxies = jobManager.Jobs.OfType<ProxyCheckJob>()
                .Select(j => j.Proxies.FirstOrDefault()).Where(p => p != null);

            // Run through all the list of proxies
            foreach (var f in firstProxies)
            {
                // If we find that a proxy which is in use by a job belongs to the group to delete
                if (ProxiesCollection.Any(p => p.Id == f.Id))
                {
                    // Prompt error and return
                    throw new Exception("Group in use by a proxy check job");
                }
            }

            // This will cascade delete all the proxies in the group
            await proxyGroupRepo.DeleteAsync(selectedGroup);

            SelectedGroupId = allGroup.Id;

            await RefreshGroupsAsync();
        }

        public async Task AddProxiesAsync(ProxiesForImportDto dto)
        {
            if (selectedGroup == allGroup)
            {
                throw new Exception("Select a group first");
            }

            var proxies = new List<Proxy>();

            foreach (var line in dto.Lines.Where(l => !string.IsNullOrEmpty(l)).Distinct())
            {
                try
                {
                    proxies.Add(Proxy.Parse(line, dto.DefaultType, dto.DefaultUsername, dto.DefaultPassword));
                }
                catch
                {

                }
            }

            var entities = proxies.Select(p => Mapper.MapProxyToProxyEntity(p)).ToList();
            var currentGroup = await proxyGroupRepo.GetAsync(selectedGroup.Id);
            proxyRepo.Attach(currentGroup);
            entities.ForEach(e => e.Group = currentGroup);

            await proxyRepo.AddAsync(entities);
            await proxyRepo.RemoveDuplicatesAsync(currentGroup.Id);
            await RefreshListAsync();
        }

        public async Task DeleteAsync(IEnumerable<ProxyEntity> proxies)
        {
            await proxyRepo.DeleteAsync(proxies);
            await RefreshListAsync();
        }

        public async Task DeleteNotWorkingAsync()
        {
            var toRemove = proxiesCollection.Where(p => p.Status == ProxyWorkingStatus.NotWorking);
            await proxyRepo.DeleteAsync(toRemove);
            await RefreshListAsync();
        }

        public async Task DeleteUntestedAsync()
        {
            var toRemove = proxiesCollection.Where(p => p.Status == ProxyWorkingStatus.Untested);
            await proxyRepo.DeleteAsync(toRemove);
            await RefreshListAsync();
        }

        public override void UpdateViewModel()
        {
            _ = RefreshListAsync();
            base.UpdateViewModel();
        }
    }
}
