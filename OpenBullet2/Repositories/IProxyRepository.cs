using OpenBullet2.Entities;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public interface IProxyRepository : IRepository<ProxyEntity>
    {
        // Removes duplicate proxies in a group
        Task RemoveDuplicates(int groupId);
    }
}
