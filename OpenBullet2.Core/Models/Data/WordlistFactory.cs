using OpenBullet2.Core.Entities;
using RuriLib.Exceptions;
using RuriLib.Models.Data;
using RuriLib.Services;
using System.Linq;

namespace OpenBullet2.Core.Models.Data
{
    /// <summary>
    /// A factory that creates a <see cref="Wordlist"/> from a <see cref="WordlistEntity"/>.
    /// </summary>
    public class WordlistFactory
    {
        private readonly RuriLibSettingsService ruriLibSettings;

        public WordlistFactory(RuriLibSettingsService ruriLibSettings)
        {
            this.ruriLibSettings = ruriLibSettings;
        }

        /// <summary>
        /// Creates a <see cref="Wordlist"/> from a <see cref="WordlistEntity"/>.
        /// </summary>
        public Wordlist FromEntity(WordlistEntity entity)
        {
            var wordlistType = ruriLibSettings.Environment.WordlistTypes
                .FirstOrDefault(w => w.Name == entity.Type);

            if (wordlistType == null)
            {
                throw new InvalidWordlistTypeException(entity.Type);
            }

            var wordlist = new Wordlist(entity.Name, entity.FileName, wordlistType, entity.Purpose, false)
            {
                Id = entity.Id,
                Total = entity.Total
            };

            return wordlist;
        }
    }
}
