using RuriLib.Models.Proxies;

namespace RuriLib.Tests.Utils;

internal sealed record ProxyContainerConnectionInfo(string Host, ushort HttpProxyPort, ushort SocksProxyPort)
{
    public Proxy CreateProxy(ProxyType type)
        => new(Host, type == ProxyType.Http ? HttpProxyPort : SocksProxyPort, type);
}
