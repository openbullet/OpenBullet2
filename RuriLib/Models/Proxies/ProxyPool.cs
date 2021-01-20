using RuriLib.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Proxies
{
    public class ProxyPool
    {
        /// <summary>The full list of proxies in the pool.</summary>
        public List<Proxy> Proxies { get; private set; } = new List<Proxy>();

        /// <summary>
        /// Initializes the proxy pool given a collection of string to be parsed as proxies.
        /// See <see cref="Proxy.Parse(string, ProxyType, string, string)"/> for format examples.
        /// </summary>
        public ProxyPool(IEnumerable<string> proxies, ProxyType defaultType = ProxyType.Http,
            string defaultUsername = "", string defaultPassword = "")
        {
            Proxies = proxies.Select(p => Proxy.Parse(p, defaultType, defaultUsername, defaultPassword)).ToList();
        }

        /// <summary>
        /// Initializes the proxy pool given a collection of CProxy objects.
        /// If <paramref name="clone"/> is true, the proxies will be first cloned and then stored in the list.
        /// </summary>
        public ProxyPool(IEnumerable<Proxy> proxies, bool clone = true)
        {
            Proxies = clone ? proxies.ToList().Clone().ToList() : proxies.ToList();
        }

        /// <summary>
        /// Removes the stored CloudflareCookies in each proxy in the pool.
        /// </summary>
        public void ClearCloudflareCookies()
            => Proxies.ForEach(p => p.CloudflareCookies = null);

        /// <summary>
        /// Sets all the BANNED and BAD proxies status to AVAILABLE and resets their Uses. It also clears the Cloudflare cookies.
        /// </summary>
        public void UnbanAll()
        {
            Proxies.ForEach(p =>
            { 
                if (p.ProxyStatus == ProxyStatus.Banned)
                {
                    p.ProxyStatus = ProxyStatus.Available;
                    p.BeingUsedBy = 0;
                    p.TotalUses = 0;
                    p.CloudflareCookies = null;
                }
            });
        }

        /// <summary>See <see cref="GetProxy(bool, int)"/>. Returns false if no proxy
        /// matching the required parameters was found and <paramref name="proxy"/> will be null.</summary>
        public bool TryGetProxy(out Proxy proxy, bool evenBusy = false, int maxUses = 0)
        {
            proxy = GetProxy(evenBusy, maxUses);
            return proxy != null;
        }

        /// <summary>
        /// Tries to return the first available proxy from the list 
        /// (or even a Busy one if <paramref name="evenBusy"/> is true).
        /// If <paramref name="maxUses"/> is not 0, the pool will try to 
        /// return a proxy that was used less than <paramref name="maxUses"/> times.
        /// Use this together with a lock if possible. Returns null if no proxy
        /// matching the required parameters was found.
        /// </summary>
        public Proxy GetProxy(bool evenBusy = false, int maxUses = 0)
        {
            IEnumerable<Proxy> possibleProxies = Proxies;

            possibleProxies = evenBusy
                ? possibleProxies.Where(p => p.ProxyStatus == ProxyStatus.Available || p.ProxyStatus == ProxyStatus.Busy)
                : possibleProxies.Where(p => p.ProxyStatus == ProxyStatus.Available);

            if (maxUses > 0)
                possibleProxies = possibleProxies.Where(p => p.TotalUses < maxUses);

            Proxy proxy = possibleProxies.FirstOrDefault();

            if (proxy != null)
            {
                proxy.BeingUsedBy++;
                proxy.ProxyStatus = ProxyStatus.Busy;
            }
            
            return proxy;
        }

        public void ReleaseProxy(Proxy proxy, bool ban = false)
        {
            proxy.TotalUses++;
            proxy.BeingUsedBy--;
            proxy.ProxyStatus = ban ? ProxyStatus.Banned : ProxyStatus.Available;
        }

        public void RemoveDuplicates()
            => Proxies = Proxies.Distinct(new GenericComparer<Proxy>()).ToList();
    }
}
