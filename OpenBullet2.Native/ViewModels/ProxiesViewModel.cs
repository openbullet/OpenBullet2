using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

namespace OpenBullet2.Native.ViewModels;

public class ProxiesViewModel : ViewModelBase
{
    private readonly ILogger<ProxiesViewModel> logger;
    private ObservableCollection<ProxyGroupEntity> proxyGroupsCollection = [];
    private ObservableCollection<ProxyEntity> proxiesCollection = [];
    private readonly JobManagerService jobManager;
    private readonly IServiceScopeFactory scopeFactory;
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
            selectedGroup = proxyGroupsCollection.FirstOrDefault(g => g.Id == value) ?? allGroup;
            OnPropertyChanged();
            _ = RefreshListAsync();
        }
    }

    public int Total => ProxiesCollection.Count;
    public int Working => ProxiesCollection.Count(p => p.Status == ProxyWorkingStatus.Working);
    public int NotWorking => ProxiesCollection.Count(p => p.Status == ProxyWorkingStatus.NotWorking);
    public bool GroupIsValid => selectedGroup != allGroup;
    public ProxyGroupEntity SelectedGroup => selectedGroup;

    public ProxiesViewModel(
        ILogger<ProxiesViewModel> logger,
        JobManagerService jobManager,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.jobManager = jobManager;
        this.scopeFactory = scopeFactory;
        ProxyGroupsCollection =
        [
            allGroup
        ];
        ProxiesCollection = [];
        selectedGroup = allGroup;
        SelectedGroupId = allGroup.Id;
    }

    public async Task InitializeAsync()
    {
        if (!initialized)
        {
            logger.LogDebug("Initializing proxies view model");
            await RefreshGroupsAsync();
            initialized = true;
        }
    }

    public async Task RefreshGroupsAsync()
    {
        SelectedGroupId = allGroup.Id;
        var entities = await WithProxyGroupRepositoryAsync(repo => repo.GetAll().ToListAsync());
        ProxyGroupsCollection = new ObservableCollection<ProxyGroupEntity>(new ProxyGroupEntity[] { allGroup }.Concat(entities));
        logger.LogDebug("Loaded {GroupCount} proxy group(s)", ProxyGroupsCollection.Count - 1);

        await RefreshListAsync();
    }

    public async Task RefreshListAsync()
    {
        var items = await WithProxyRepositoryAsync(repo => selectedGroup == allGroup
            ? repo.GetAll().ToListAsync()
            : repo.GetAll()
                .Include(p => p.Group)
                .Where(p => p.Group != null && p.Group.Id == selectedGroup.Id)
                .ToListAsync());

        ProxiesCollection = new ObservableCollection<ProxyEntity>(items);
        logger.LogDebug("Loaded {ProxyCount} proxies for group {GroupName}", ProxiesCollection.Count, selectedGroup.Name);
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(Working));
        OnPropertyChanged(nameof(NotWorking));
    }

    public Task AddGroupAsync(ProxyGroupEntity group)
    {
        ProxyGroupsCollection.Add(group);
        SelectedGroupId = allGroup.Id;
        logger.LogInformation("Adding proxy group {GroupName}", group.Name);

        return WithProxyGroupRepositoryAsync(repo => repo.AddAsync(group));
    }

    public async Task EditGroupAsync(ProxyGroupEntity group)
    {
        logger.LogInformation("Editing proxy group {GroupId} ({GroupName})", group.Id, group.Name);
        await WithProxyGroupRepositoryAsync(repo => repo.UpdateAsync(group));
        await RefreshGroupsAsync();
    }

    public async Task DeleteSelectedGroupAsync()
    {
        if (selectedGroup == allGroup)
        {
            logger.LogWarning("Attempted to delete proxy group while no valid group was selected");
            throw new Exception("Select a group first");
        }

        var firstProxies = jobManager.Jobs.OfType<ProxyCheckJob>()
            .Select(j => j.Proxies?.FirstOrDefault())
            .Where(p => p != null);

        // Run through all the list of proxies
        foreach (var f in firstProxies)
        {
            // If we find that a proxy which is in use by a job belongs to the group to delete
            if (f != null && ProxiesCollection.Any(p => p.Id == f.Id))
            {
                // Prompt error and return
                logger.LogWarning("Blocked deletion of proxy group {GroupId} because it is used by a proxy check job",
                    selectedGroup.Id);
                throw new Exception("Group in use by a proxy check job");
            }
        }

        var deletedGroupId = selectedGroup.Id;
        var deletedGroupName = selectedGroup.Name;

        // This will cascade delete all the proxies in the group
        await WithProxyGroupRepositoryAsync(async repo =>
        {
            var group = await repo.GetAsync(selectedGroup.Id)
                ?? throw new InvalidOperationException("Selected proxy group was not found");
            await repo.DeleteAsync(group);
        });

        SelectedGroupId = allGroup.Id;

        await RefreshGroupsAsync();
        logger.LogInformation("Deleted proxy group {GroupId} ({GroupName})", deletedGroupId, deletedGroupName);
    }

    public async Task AddProxiesAsync(ProxiesForImportDto dto)
    {
        if (selectedGroup == allGroup)
        {
            logger.LogWarning("Attempted to import proxies while no valid group was selected");
            throw new Exception("Select a group first");
        }

        var proxies = new List<Proxy>();
        var invalidCount = 0;

        foreach (var line in dto.Lines.Where(l => !string.IsNullOrEmpty(l)).Distinct())
        {
            try
            {
                proxies.Add(Proxy.Parse(line, dto.DefaultType, dto.DefaultUsername, dto.DefaultPassword));
            }
            catch (Exception ex)
            {
                invalidCount++;
                logger.LogDebug(ex, "Failed to parse proxy line during import for group {GroupId}", selectedGroup.Id);
            }
        }

        var groupId = selectedGroup.Id;
        var groupName = selectedGroup.Name;
        var entities = proxies.Select(ProxyEntityMapper.MapProxyToProxyEntity).ToList();
        await WithRepositoriesAsync(async (proxyGroupRepo, proxyRepo) =>
        {
            var currentGroup = await proxyGroupRepo.GetAsync(groupId)
                ?? throw new InvalidOperationException("Selected proxy group was not found");
            proxyRepo.Attach(currentGroup);
            entities.ForEach(e => e.Group = currentGroup);

            await proxyRepo.AddAsync(entities);
            await proxyRepo.RemoveDuplicatesAsync(currentGroup.Id);
        });
        await RefreshListAsync();
        logger.LogInformation(
            "Imported {ImportedCount} proxies into group {GroupId} ({GroupName}), skipped {InvalidCount} invalid line(s)",
            entities.Count, groupId, groupName, invalidCount);
    }

    public async Task DeleteAsync(IEnumerable<ProxyEntity> proxies)
    {
        var proxyList = proxies.ToList();
        await WithProxyRepositoryAsync(repo => repo.DeleteAsync(proxyList));
        await RefreshListAsync();
        logger.LogInformation("Deleted {ProxyCount} selected proxies from group {GroupName}",
            proxyList.Count, selectedGroup.Name);
    }

    public async Task DeleteNotWorkingAsync()
    {
        var toRemove = ProxiesCollection.Where(p => p.Status == ProxyWorkingStatus.NotWorking).ToList();
        await WithProxyRepositoryAsync(repo => repo.DeleteAsync(toRemove));
        await RefreshListAsync();
        logger.LogInformation("Deleted {ProxyCount} not working proxies from group {GroupName}",
            toRemove.Count, selectedGroup.Name);
    }

    public async Task DeleteUntestedAsync()
    {
        var toRemove = ProxiesCollection.Where(p => p.Status == ProxyWorkingStatus.Untested).ToList();
        await WithProxyRepositoryAsync(repo => repo.DeleteAsync(toRemove));
        await RefreshListAsync();
        logger.LogInformation("Deleted {ProxyCount} untested proxies from group {GroupName}",
            toRemove.Count, selectedGroup.Name);
    }

    public async Task DeleteLowQualityAsync(DeleteLowQualityProxiesDto dto)
    {
        var qualities = new List<ProxyQuality>();

        if (dto.DeleteUnknown)
        {
            qualities.Add(ProxyQuality.Unknown);
        }

        if (dto.DeleteTransparent)
        {
            qualities.Add(ProxyQuality.Transparent);
        }

        if (dto.DeleteAnonymous)
        {
            qualities.Add(ProxyQuality.Anonymous);
        }

        var toRemove = ProxiesCollection.Where(p => qualities.Contains(p.Quality)).ToList();
        await WithProxyRepositoryAsync(repo => repo.DeleteAsync(toRemove));
        await RefreshListAsync();
        logger.LogInformation("Deleted {ProxyCount} low-quality proxies from group {GroupName}",
            toRemove.Count, selectedGroup.Name);
    }

    public override void UpdateViewModel()
    {
        _ = RefreshListAsync();
        base.UpdateViewModel();
    }

    private async Task WithRepositoriesAsync(Func<IProxyGroupRepository, IProxyRepository, Task> action)
    {
        using var scope = scopeFactory.CreateScope();
        var proxyGroupRepo = scope.ServiceProvider.GetRequiredService<IProxyGroupRepository>();
        var proxyRepo = scope.ServiceProvider.GetRequiredService<IProxyRepository>();
        await action(proxyGroupRepo, proxyRepo);
    }

    private async Task WithProxyGroupRepositoryAsync(Func<IProxyGroupRepository, Task> action)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProxyGroupRepository>();
        await action(repo);
    }

    private async Task<T> WithProxyGroupRepositoryAsync<T>(Func<IProxyGroupRepository, Task<T>> action)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProxyGroupRepository>();
        return await action(repo);
    }

    private async Task WithProxyRepositoryAsync(Func<IProxyRepository, Task> action)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProxyRepository>();
        await action(repo);
    }

    private async Task<T> WithProxyRepositoryAsync<T>(Func<IProxyRepository, Task<T>> action)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProxyRepository>();
        return await action(repo);
    }
}
