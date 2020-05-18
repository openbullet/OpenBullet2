using OpenBullet2.Entities;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Helpers
{
    // TODO: Refactor these methods, make it so that you can call Map<TInput,TOutput>
    public static class Mapper
    {
        public static ProxyEntity MapProxyToProxyEntity(Proxy proxy)
        {
            return new ProxyEntity
            {
                Country = proxy.Country,
                Host = proxy.Host,
                Port = proxy.Port,
                LastChecked = proxy.LastChecked,
                Username = proxy.Username,
                Password = proxy.Password,
                Ping = proxy.Ping,
                Status = proxy.WorkingStatus,
                Type = proxy.Type
            };
        }
    }
}
