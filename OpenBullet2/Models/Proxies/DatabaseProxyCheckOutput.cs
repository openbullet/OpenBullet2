using OpenBullet2.Repositories;
using RuriLib.Models.Proxies;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Proxies
{
    public class DatabaseProxyCheckOutput : IProxyCheckOutput
    {
        private readonly IProxyRepository proxyRepo;

        public DatabaseProxyCheckOutput(IProxyRepository proxyRepo)
        {
            this.proxyRepo = proxyRepo;
        }

        public async Task Store(Proxy proxy)
        {
            var entity = await proxyRepo.Get(proxy.Id);
            entity.Country = proxy.Country;
            entity.LastChecked = proxy.LastChecked;
            entity.Ping = proxy.Ping;
            entity.Status = proxy.WorkingStatus;

            await proxyRepo.Update(entity);
        }
    }
}
