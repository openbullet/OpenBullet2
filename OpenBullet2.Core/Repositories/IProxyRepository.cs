using OpenBullet2.Core.Entities;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories;

/// <summary>
/// Stores proxies.
/// </summary>
public interface IProxyRepository : IRepository<ProxyEntity>
{
    /// <summary>
    /// Removes duplicate proxies that belong to the group with a given <paramref name="groupId"/> from the Proxies table.
    /// Duplication is checked on type, host, port, username and password.
    /// Returns the number of removed entries.
    /// </summary>
    Task<int> RemoveDuplicatesAsync(int groupId);
}
