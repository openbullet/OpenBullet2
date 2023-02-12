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
                _ = RefreshList();
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

        public async Task Initialize()
        {
            if (!initialized)
            {
                await RefreshGroups();
                initialized = true;
            }
        }

        public async Task RefreshGroups()
        {
            SelectedGroupId = allGroup.Id;
            var entities = await proxyGroupRepo.GetAll().ToListAsync();
            ProxyGroupsCollection = new ObservableCollection<ProxyGroupEntity>(new ProxyGroupEntity[] { allGroup }.Concat(entities));

            await RefreshList();
        }

        public async Task RefreshList()
        {
            var items = selectedGroup == allGroup
                ? await proxyRepo.GetAll().ToListAsync()
                : await proxyRepo.GetAll().Include(p => p.Group).Where(p => p.Group.Id == selectedGroup.Id).ToListAsync();

            ProxiesCollection = new ObservableCollection<ProxyEntity>(items);
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Working));
            OnPropertyChanged(nameof(NotWorking));
        }

        public Task AddGroup(ProxyGroupEntity group)
        {
            ProxyGroupsCollection.Add(group);
            SelectedGroupId = allGroup.Id;

            return proxyGroupRepo.Add(group);
        }

        public async Task EditGroup(ProxyGroupEntity group)
        {
            await proxyGroupRepo.Update(group);
            await RefreshGroups();
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

            await proxyRepo.Delete(ProxiesCollection);
            await proxyGroupRepo.Delete(selectedGroup);

            SelectedGroupId = allGroup.Id;

            await RefreshGroups();
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
            var currentGroup = await proxyGroupRepo.Get(selectedGroup.Id);
            proxyRepo.Attach(currentGroup);
            entities.ForEach(e => e.Group = currentGroup);

            await proxyRepo.Add(entities);
            await proxyRepo.RemoveDuplicates(currentGroup.Id);
            await RefreshList();
        }

        public async Task Delete(IEnumerable<ProxyEntity> proxies)
        {
            await proxyRepo.Delete(proxies);
            await RefreshList();
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

        public override void UpdateViewModel()
        {
            _ = RefreshList();
            base.UpdateViewModel();
        }
    }
}
