using OpenBullet2.Entities;
using OpenBullet2.Repositories;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Hits;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Hits
{
    public class DatabaseHitOutput : IHitOutput
    {
        private readonly IHitRepository hitRepo;

        public DatabaseHitOutput(IHitRepository hitRepo)
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
                ConfigCategory = hit.Config.Metadata.Category,
                OwnerId = hit.OwnerId
            };

            if (hit.DataPool is WordlistDataPool)
            {
                var wordlist = (hit.DataPool as WordlistDataPool).Wordlist;
                entity.WordlistId = wordlist.Id;
                entity.WordlistName = wordlist.Name;
            }

            await hitRepo.Add(entity);
        }
    }
}
