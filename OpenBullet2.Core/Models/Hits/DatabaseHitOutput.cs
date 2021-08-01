using OpenBullet2.Core.Services;
using RuriLib.Models.Hits;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Models.Hits
{
    /// <summary>
    /// A hit output that stores hits to a database.
    /// </summary>
    public class DatabaseHitOutput : IHitOutput
    {
        private readonly HitStorageService hitStorage;

        public DatabaseHitOutput(HitStorageService hitStorage)
        {
            this.hitStorage = hitStorage;
        }

        /// <inheritdoc/>
        public Task Store(Hit hit)
            => hitStorage.Store(hit);
    }
}
