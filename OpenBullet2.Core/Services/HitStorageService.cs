using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Hits;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Services;

/// <summary>
/// Stores hits to an <see cref="IHitRepository"/> in a thread-safe manner.
/// </summary>
public class HitStorageService : IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;

    public HitStorageService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Stores a hit in a thread-safe manner.
    /// </summary>
    public async Task StoreAsync(Hit hit)
    {
        using var scope = _scopeFactory.CreateScope();

        // TODO: If this is too slow for a huge amount of hits, since
        // we are basically creating a new context each time, we might
        // explore using raw queries to insert data which would be way faster.
        var hitRepo = scope.ServiceProvider.GetRequiredService<IHitRepository>();

        var entity = new HitEntity
        {
            CapturedData = hit.CapturedDataString,
            Data = hit.DataString,
            Date = hit.Date,
            Proxy = hit.ProxyString,
            Type = hit.Type,
            ConfigId = hit.Config.Id,
            ConfigName = hit.Config.Metadata.Name,
            ConfigCategory = hit.Config.Metadata.Category,
            OwnerId = hit.OwnerId
        };

        switch (hit.DataPool)
        {
            case WordlistDataPool wordlistDataPool:
                entity.WordlistId = wordlistDataPool.Wordlist.Id;
                entity.WordlistName = wordlistDataPool.Wordlist.Name;
                break;

            // The following are not actual wordlists but it can help identify which pool was used
            case FileDataPool fileDataPool:
                entity.WordlistId = fileDataPool.POOL_CODE;
                entity.WordlistName = fileDataPool.FileName;
                break;

            case RangeDataPool rangeDataPool:
                entity.WordlistId = rangeDataPool.POOL_CODE;
                entity.WordlistName = $"{rangeDataPool.Start}|{rangeDataPool.Amount}|{rangeDataPool.Step}|{rangeDataPool.Pad}";
                break;

            case CombinationsDataPool combationsDataPool:
                entity.WordlistId = combationsDataPool.POOL_CODE;
                entity.WordlistName = $"{combationsDataPool.Length}|{combationsDataPool.CharSet}";
                break;

            case InfiniteDataPool infiniteDataPool:
                entity.WordlistId = infiniteDataPool.POOL_CODE;
                break;
        }

        await hitRepo.AddAsync(entity);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
