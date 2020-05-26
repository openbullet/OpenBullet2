using Newtonsoft.Json;
using OpenBullet2.Entities;
using OpenBullet2.Repositories;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Hits;
using RuriLib.Models.Variables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Hits
{
    public class DatabaseHitOutput : IHitOutput
    {
        [JsonIgnore]
        private readonly SingletonDbHitRepository hitRepo;

        public DatabaseHitOutput(SingletonDbHitRepository hitRepo)
        {
            this.hitRepo = hitRepo;
        }

        public async Task Store(Hit hit)
        {
            var entity = new HitEntity
            {
                CapturedData = hit.CapturedDataString,
                Data = hit.DataString,
                Date = hit.Date,
                Proxy = hit.ProxyString,
                Type = hit.Type,
                ConfigId = hit.Config.Id,
                ConfigName = hit.Config.Metadata.Name,
                ConfigCategory = hit.Config.Metadata.Category
            };

            if (hit.DataPool is WordlistDataPool)
            {
                var wordlist = (hit.DataPool as WordlistDataPool).Wordlist;
                entity.WordlistId = wordlist.Id;
                entity.WordlistName = wordlist.Name;
            }

            await hitRepo.Store(entity);
        }
    }
}
