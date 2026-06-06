using System.Threading.Tasks;

namespace RuriLib.Models.Proxies;

/// <summary>
/// Resolves geolocation information for proxy hosts.
/// </summary>
public interface IProxyGeolocationProvider
{
    /// <summary>
    /// Resolves the country or location label for a proxy host.
    /// </summary>
    /// <param name="host">The proxy host name or IP address.</param>
    /// <returns>A task that returns the resolved location string.</returns>
    Task<string> GeolocateAsync(string host);
}
