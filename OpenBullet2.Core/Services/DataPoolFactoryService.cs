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
    private readonly IWordlistRepository _wordlistRepo;
    private readonly RuriLibSettingsService _ruriLibSettings;

    public DataPoolFactoryService(IWordlistRepository wordlistRepo, RuriLibSettingsService ruriLibSettings)
    {
        _wordlistRepo = wordlistRepo;
        _ruriLibSettings = ruriLibSettings;
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
            Console.WriteLine($"Exception while loading data pool. {ex.Message}");
            return new InfiniteDataPool();
        }
    }

    private async Task<DataPool> MakeWordlistDataPoolAsync(WordlistDataPoolOptions options)
    {
        var entity = await _wordlistRepo.GetAsync(options.WordlistId);

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
