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
        private List<ProxyGroupEntity> proxyGroups;
        private ObservableCollection<ProxyEntity> proxiesCollection;
        private readonly IProxyGroupRepository proxyGroupRepo;
        private readonly IProxyRepository proxyRepo;
        private readonly JobManagerService jobManager;
        private bool initialized;
        private int selectedGroupId;
        private ProxyGroupEntity selectedGroup;
        private readonly ProxyGroupEntity allGroup = new() { Id = -1, Name = "All" };

        public event Action<IEnumerable<int>> ProxyGroupsChanged;

        public ObservableCollection<ProxyEntity> ProxiesCollection
        {
            get => proxiesCollection;
            private set
            {
                proxiesCollection = value;
                OnPropertyChanged();
            }
        }

        public int SelectedGroupId
        {
            get => selectedGroupId;
            set
            {
                selectedGroupId = value;
                _ = RefreshList();
                OnPropertyChanged();
            }
        }

        public int Total => ProxiesCollection.Count;
        public bool GroupIsValid => selectedGroup != allGroup;
        public ProxyGroupEntity SelectedGroup => selectedGroup;

        public ProxiesViewModel()
        {
            proxyGroupRepo = SP.GetService<IProxyGroupRepository>();
            proxyRepo = SP.GetService<IProxyRepository>();
            jobManager = SP.GetService<JobManagerService>();
            proxyGroups = new List<ProxyGroupEntity>
            {
                allGroup
            };
            ProxiesCollection = new ObservableCollection<ProxyEntity>();
            selectedGroup = allGroup;
        }

        public async Task Initialize()
        {
            if (!initialized)
            {
                await RefreshGroups();
                await RefreshList();
                initialized = true;
            }
        }

        public async Task RefreshGroups()
        {
            SelectedGroupId = allGroup.Id;
            proxyGroups = await proxyGroupRepo.GetAll().ToListAsync();
            proxyGroups.Insert(0, selectedGroup);
            ProxyGroupsChanged?.Invoke(proxyGroups.Select(g => g.Id));
        }

        public async Task RefreshList()
        {
            var items = selectedGroup == null
                ? await proxyRepo.GetAll().ToListAsync()
                : await proxyRepo.GetAll().Include(p => p.Group).Where(p => p.Group.Id == selectedGroup.Id).ToListAsync();

            ProxiesCollection = new ObservableCollection<ProxyEntity>(items);
        }

        public async Task AddGroup(ProxyGroupEntity group)
        {
            ProxyGroupsChanged?.Invoke(proxyGroups.Select(g => g.Id));
            proxyGroups.Add(group);
            SelectedGroupId = group.Id;
            await proxyGroupRepo.Add(group);
        }

        public async Task EditGroup(ProxyGroupEntity group)
        {
            await proxyGroupRepo.Update(group);
            await RefreshGroups();
            await RefreshList();
        }

        public async Task DeleteSelectedGroup()
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

            var toRemove = selectedGroup;
            SelectedGroupId = allGroup.Id;
            proxyGroups.Remove(toRemove);
            await proxyGroupRepo.Delete(toRemove);
            await proxyRepo.Delete(ProxiesCollection);
            await RefreshList();
        }

        public async Task AddProxies(ProxiesForImportDto dto)
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
            var currentGroup = await proxyGroupRepo.Get(SelectedGroupId);
            entities.ForEach(e => e.Group = currentGroup);

            await proxyRepo.Add(entities);
            await proxyRepo.RemoveDuplicates(currentGroup.Id);
            await RefreshList();
        }

        public async Task Delete(ProxyEntity proxy)
        {
            ProxiesCollection.Remove(proxy);
            await proxyRepo.Delete(proxy);
        }

        public async Task DeleteNotWorking()
        {
            var toRemove = proxiesCollection.Where(p => p.Status == ProxyWorkingStatus.NotWorking);
            await proxyRepo.Delete(toRemove);
            await RefreshList();
        }

        public async Task DeleteUntested()
        {
            var toRemove = proxiesCollection.Where(p => p.Status == ProxyWorkingStatus.Untested);
            await proxyRepo.Delete(toRemove);
            await RefreshList();
        }
    }
}
