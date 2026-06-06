using OpenBullet2.Core.Entities;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Core.Helpers;

public static class ProxyEntityMapper
{
    /// <summary>
    /// Maps a <see cref="Proxy"/> to a <see cref="ProxyEntity"/>.
    /// </summary>
    public static ProxyEntity MapProxyToProxyEntity(Proxy proxy) => new()
    {
        Country = proxy.Country,
        Host = proxy.Host,
        Port = proxy.Port,
        LastChecked = proxy.LastChecked ?? default,
        Username = proxy.Username,
        Password = proxy.Password,
        Ping = proxy.Ping,
        Quality = proxy.Quality,
        Status = proxy.WorkingStatus,
        Type = proxy.Type
    };
}
