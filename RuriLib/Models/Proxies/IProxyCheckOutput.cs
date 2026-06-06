using System.Threading.Tasks;

namespace RuriLib.Models.Proxies;

/// <summary>
/// Stores checked proxies in an output sink.
/// </summary>
public interface IProxyCheckOutput
{
    /// <summary>
    /// Stores a checked proxy.
    /// </summary>
    /// <param name="proxy">The proxy to store.</param>
    /// <returns>A task that completes when the proxy has been stored.</returns>
    Task StoreAsync(Proxy proxy);
}
