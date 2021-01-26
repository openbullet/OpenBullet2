namespace RuriLib.Models.Proxies
{
    public class ProxyPoolOptions
    {
        public ProxyType[] AllowedTypes { get; set; } 
            = new ProxyType[] { ProxyType.Http, ProxyType.Socks4, ProxyType.Socks5 };
    }
}
