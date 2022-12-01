using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Models.Proxies.Sources
{
    /// <summary>
    /// A proxy source that gets proxies from a group of a <see cref="IProxyGroupRepository"/>.
    /// </summary>
    public class GroupProxySource : ProxySource, IDisposable
    {
        private readonly ProxyReloadService reloadService;

        /// <summary>
        /// The ID of the group in the <see cref="IProxyGroupRepository"/>.
        /// </summary>
        public int GroupId { get; set; }

        public GroupProxySource(int groupId, ProxyReloadService reloadService)
        {
            GroupId = groupId;
            this.reloadService = reloadService;
        }

        /// <inheritdoc/>
        public async override Task<IEnumerable<Proxy>> GetAllAsync(CancellationToken cancellationToken = default)
            => await reloadService.ReloadAsync(GroupId, UserId, cancellationToken).ConfigureAwait(false);

        public override void Dispose()
        {
            base.Dispose();
            reloadService?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
