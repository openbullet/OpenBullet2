using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Repositories;
using RuriLib.Models.Data;
using RuriLib.Models.Data.DataPools;
using RuriLib.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using OpenBullet2.Core.Exceptions;

namespace OpenBullet2.Core.Services;

/// <summary>
/// Factory that creates a <see cref="DataPool"/> from <see cref="DataPoolOptions"/>.
/// </summary>
public class DataPoolFactoryService
{
    private readonly RuriLibSettingsService _ruriLibSettings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataPoolFactoryService> _logger;

    public DataPoolFactoryService(
        IServiceScopeFactory scopeFactory,
        RuriLibSettingsService ruriLibSettings,
        ILogger<DataPoolFactoryService>? logger = null)
    {
        _scopeFactory = scopeFactory;
        _ruriLibSettings = ruriLibSettings;
        _logger = logger ?? NullLogger<DataPoolFactoryService>.Instance;
    }

    /// <summary>
    /// Creates a <see cref="DataPool"/> from <see cref="DataPoolOptions"/>.
    /// </summary>
    public async Task<DataPool> FromOptionsAsync(DataPoolOptions options)
    {
        try
        {
            return options switch
            {
                InfiniteDataPoolOptions x => new InfiniteDataPool(x.WordlistType),
                CombinationsDataPoolOptions x => new CombinationsDataPool(x.CharSet, x.Length, x.WordlistType),
                RangeDataPoolOptions x => new RangeDataPool(x.Start, x.Amount, x.Step, x.Pad, x.WordlistType),
                FileDataPoolOptions x => new FileDataPool(x.FileName, x.WordlistType),
                WordlistDataPoolOptions x => await MakeWordlistDataPoolAsync(x),
                _ => throw new NotImplementedException()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load data pool from options type {OptionsType}, falling back to InfiniteDataPool",
                options.GetType().Name);
            return new InfiniteDataPool();
        }
    }

    private async Task<DataPool> MakeWordlistDataPoolAsync(WordlistDataPoolOptions options)
    {
        using var scope = _scopeFactory.CreateScope();
        var wordlistRepo = scope.ServiceProvider.GetRequiredService<IWordlistRepository>();
        var entity = await wordlistRepo.GetAsync(options.WordlistId);

        // If the entity was deleted
        if (entity == null)
        {
            throw new EntityNotFoundException($"Wordlist entity not found: {options.WordlistId}");
        }

        if (!File.Exists(entity.FileName))
        {
            throw new EntityNotFoundException($"Wordlist file not found: {entity.FileName}");
        }

        var factory = new WordlistFactory(_ruriLibSettings);
        return new WordlistDataPool(factory.FromEntity(entity));
    }
}
