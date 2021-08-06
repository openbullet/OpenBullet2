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
        private readonly IProxyGroupRepository proxyGroupRepo;
        private readonly IProxyRepository proxyRepo;
        private readonly JobManagerService jobManager;
        private bool initialized = false;
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
            jobManager = SP.GetService<JobManagerService>();
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

        public async Task RefreshGroups()
        {
            SelectedGroup = allGroup;
            var groups = await proxyGroupRepo.GetAll().ToListAsync();
            groups.Insert(0, SelectedGroup);
            ProxyGroupsCollection = new ObservableCollection<ProxyGroupEntity>(groups);
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
            ProxyGroupsCollection.Add(group);
            SelectedGroup = group;
            await proxyGroupRepo.Add(group);
        }

        public async Task EditGroup(ProxyGroupEntity group) => await proxyGroupRepo.Update(group);

        public async Task DeleteGroup()
        {
            if (SelectedGroup == allGroup)
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

            var toRemove = SelectedGroup;
            SelectedGroup = allGroup;
            ProxyGroupsCollection.Remove(toRemove);
            await proxyGroupRepo.Delete(toRemove);
            await proxyRepo.Delete(ProxiesCollection);
            await RefreshList();
        }

        public async Task AddProxies(ProxiesForImportDto dto)
        {
            if (SelectedGroup == allGroup)
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
            entities.ForEach(e => e.Group = SelectedGroup);

            await proxyRepo.Add(entities);
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
    }
}
