using OpenBullet2.Core.Repositories;
using RuriLib.Models.Proxies;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OpenBullet2.Core.Models.Proxies;

/// <summary>
/// A proxy check output that writes proxies to an <see cref="IProxyRepository"/>.
/// </summary>
public class DatabaseProxyCheckOutput : IProxyCheckOutput, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseProxyCheckOutput> _logger;
    private readonly SemaphoreSlim _semaphore;

    public DatabaseProxyCheckOutput(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        using var scope = _scopeFactory.CreateScope();
        _logger = scope.ServiceProvider.GetService<ILogger<DatabaseProxyCheckOutput>>()
            ?? NullLogger<DatabaseProxyCheckOutput>.Instance;
        _semaphore = new SemaphoreSlim(1, 1);
    }

    /// <inheritdoc/>
    public async Task StoreAsync(Proxy proxy)
    {
        try
        {
            // Only allow updating one proxy at a time (multiple threads should
            // not use the same DbContext at the same time).
            await _semaphore.WaitAsync();

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var proxyRepo = scope.ServiceProvider.GetRequiredService<IProxyRepository>();
                var entity = await proxyRepo.GetAsync(proxy.Id);
                if (entity is null)
                {
                    return;
                }

                entity.Country = proxy.Country;
                entity.LastChecked = proxy.LastChecked ?? default;
                entity.Ping = proxy.Ping;
                entity.Quality = proxy.Quality;
                entity.Status = proxy.WorkingStatus;

                await proxyRepo.UpdateAsync(entity);
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
            _logger.LogDebug(ex,
                "Skipped saving proxy {ProxyId} to the database because the proxy, job or scope was no longer available",
                proxy.Id);
        }
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
        GC.SuppressFinalize(this);
    }
}
