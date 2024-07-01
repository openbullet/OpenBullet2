using OpenBullet2.Core.Repositories;
using RuriLib.Models.Proxies;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OpenBullet2.Core.Models.Proxies;

/// <summary>
/// A proxy check output that writes proxies to an <see cref="IProxyRepository"/>.
/// </summary>
public class DatabaseProxyCheckOutput : IProxyCheckOutput, IDisposable
{
    private readonly IServiceScope _scope;
    private readonly IProxyRepository _proxyRepo;
    private readonly SemaphoreSlim _semaphore;

    public DatabaseProxyCheckOutput(IServiceScopeFactory scopeFactory)
    {
        _scope = scopeFactory.CreateScope();
        _proxyRepo = _scope.ServiceProvider.GetRequiredService<IProxyRepository>();
        _semaphore = new SemaphoreSlim(1, 1);
    }

    /// <inheritdoc/>
    public async Task StoreAsync(Proxy proxy)
    {
        try
        {
            var entity = await _proxyRepo.GetAsync(proxy.Id);
            entity.Country = proxy.Country;
            entity.LastChecked = proxy.LastChecked;
            entity.Ping = proxy.Ping;
            entity.Status = proxy.WorkingStatus;

            // Only allow updating one proxy at a time (multiple threads should
            // not use the same DbContext at the same time).
            await _semaphore.WaitAsync();

            try
            {
                await _proxyRepo.UpdateAsync(entity);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            /* 
             * If we are here it means a few possible things
             * - we deleted the job but the parallelizer was still running
             * - the original proxy was deleted (e.g. from the proxy tab)
             * - the scope was disposed for some reason
             * 
             * In any case we don't want to save anything to the database.
             */
            
            // TODO: Turn this into a log message using a logger
            Console.WriteLine($"Error while saving proxy {proxy.Id} to the database: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
        _scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}
