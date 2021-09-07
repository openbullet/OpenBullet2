using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Repositories;
using RuriLib.Models.Data;
using RuriLib.Models.Data.DataPools;
using RuriLib.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Services
{
    /// <summary>
    /// Factory that creates a <see cref="DataPool"/> from <see cref="DataPoolOptions"/>.
    /// </summary>
    public class DataPoolFactoryService
    {
        private readonly IWordlistRepository wordlistRepo;
        private readonly RuriLibSettingsService ruriLibSettings;

        public DataPoolFactoryService(IWordlistRepository wordlistRepo, RuriLibSettingsService ruriLibSettings)
        {
            this.wordlistRepo = wordlistRepo;
            this.ruriLibSettings = ruriLibSettings;
        }

        /// <summary>
        /// Creates a <see cref="DataPool"/> from <see cref="DataPoolOptions"/>.
        /// </summary>
        public async Task<DataPool> FromOptions(DataPoolOptions options)
        {
            try
            {
                return options switch
                {
                    InfiniteDataPoolOptions x => new InfiniteDataPool(x.WordlistType),
                    CombinationsDataPoolOptions x => new CombinationsDataPool(x.CharSet, x.Length, x.WordlistType),
                    RangeDataPoolOptions x => new RangeDataPool(x.Start, x.Amount, x.Step, x.Pad, x.WordlistType),
                    FileDataPoolOptions x => new FileDataPool(x.FileName, x.WordlistType),
                    WordlistDataPoolOptions x => await MakeWordlistDataPool(x),
                    _ => throw new NotImplementedException()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while loading data pool. {ex.Message}");
                return new InfiniteDataPool();
            }
        }

        private async Task<DataPool> MakeWordlistDataPool(WordlistDataPoolOptions options)
        {
            var entity = await wordlistRepo.Get(options.WordlistId);

            // If the entity was deleted
            if (entity == null)
            {
                throw new Exception($"Wordlist entity not found: {options.WordlistId}");
            }

            if (!File.Exists(entity.FileName))
            {
                throw new Exception($"Wordlist file not found: {entity.FileName}");
            }

            var factory = new WordlistFactory(ruriLibSettings);
            return new WordlistDataPool(factory.FromEntity(entity));
        }
    }
}
