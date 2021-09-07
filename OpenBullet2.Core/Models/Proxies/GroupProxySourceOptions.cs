using OpenBullet2.Core.Models.Proxies.Sources;
using OpenBullet2.Core.Repositories;

namespace OpenBullet2.Core.Models.Proxies
{
    /// <summary>
    /// Options for a <see cref="GroupProxySource"/>
    /// </summary>
    public class GroupProxySourceOptions : ProxySourceOptions
    {
        /// <summary>
        /// The ID of the proxy group, as stored in the <see cref="IProxyGroupRepository"/>.
        /// </summary>
        public int GroupId { get; set; } = -1;
    }
}
