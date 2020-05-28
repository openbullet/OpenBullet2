using OpenBullet2.Repositories;
using RuriLib.Models.Data;
using RuriLib.Models.Data.DataPools;
using RuriLib.Services;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Data
{
    public class DataPoolFactory
    {
        private readonly IWordlistRepository wordlistRepo;
        private readonly RuriLibSettingsService ruriLibSettings;

        public DataPoolFactory(IWordlistRepository wordlistRepo, RuriLibSettingsService ruriLibSettings)
        {
            this.wordlistRepo = wordlistRepo;
            this.ruriLibSettings = ruriLibSettings;
        }

        public async Task<DataPool> FromOptions(DataPoolOptions options)
        {
            DataPool pool = options switch
            {
                InfiniteDataPoolOptions _ => new InfiniteDataPool(),
                CombinationsDataPoolOptions x => new CombinationsDataPool(x.CharSet, x.Length),
                RangeDataPoolOptions x => new RangeDataPool(x.Start, x.Amount, x.Step, x.Pad),
                FileDataPoolOptions x => new FileDataPool(x.FileName),
                WordlistDataPoolOptions x => await MakeWordlistDataPool(x),
                _ => throw new NotImplementedException()
            };

            return pool;
        }

        private async Task<WordlistDataPool> MakeWordlistDataPool(WordlistDataPoolOptions options)
        {
            var entity = await wordlistRepo.Get(options.WordlistId);
            var factory = new WordlistFactory(ruriLibSettings);
            return new WordlistDataPool(factory.FromEntity(entity));
        }
    }
}
