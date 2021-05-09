using OpenBullet2.Services;
using RuriLib.Models.Hits;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Hits
{
    public class DatabaseHitOutput : IHitOutput
    {
        private readonly HitStorageService hitStorage;

        public DatabaseHitOutput(HitStorageService hitStorage)
        {
            this.hitStorage = hitStorage;
        }

        public Task Store(Hit hit)
            => hitStorage.Store(hit);
    }
}
