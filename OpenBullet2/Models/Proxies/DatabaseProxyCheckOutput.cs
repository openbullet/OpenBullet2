using OpenBullet2.Repositories;
using RuriLib.Models.Proxies;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Proxies
{
    public class DatabaseProxyCheckOutput : IProxyCheckOutput
    {
        private readonly IProxyRepository proxyRepo;
        private object dbLock = new object();

        public DatabaseProxyCheckOutput(IProxyRepository proxyRepo)
        {
            this.proxyRepo = proxyRepo;
        }

        public async Task Store(Proxy proxy)
        {
            while (!Monitor.TryEnter(dbLock))
                await Task.Delay(100);

            try
            {
                var entity = await proxyRepo.Get(proxy.Id);
                entity.Country = proxy.Country;
                entity.LastChecked = proxy.LastChecked;
                entity.Ping = proxy.Ping;
                entity.Status = proxy.WorkingStatus;

                await proxyRepo.Update(entity);
            }
            catch (ObjectDisposedException)
            {
                // If we are here, it means we deleted the job but the task manager was still running
                // so we don't need to put anything in the database and we can safely ignore the exception.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Monitor.Exit(dbLock);
            }
        }
    }
}
