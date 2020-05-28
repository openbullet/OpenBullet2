using OpenBullet2.Entities;
using RuriLib.Models.Data;
using RuriLib.Services;
using System.Linq;

namespace OpenBullet2.Models.Data
{
    public class WordlistFactory
    {
        private readonly RuriLibSettingsService ruriLibSettings;

        public WordlistFactory(RuriLibSettingsService ruriLibSettings)
        {
            this.ruriLibSettings = ruriLibSettings;
        }

        public Wordlist FromEntity(WordlistEntity entity)
        {
            var wordlistType = ruriLibSettings.Environment.WordlistTypes
                .First(w => w.Name == entity.Type);

            var wordlist = new Wordlist(entity.Name, entity.FileName, wordlistType, entity.Purpose, false)
            {
                Id = entity.Id,
                Total = entity.Total
            };

            return wordlist;
        }
    }
}
