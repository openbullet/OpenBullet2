using RuriLib.Extensions;
using RuriLib.Models.Proxies.ProxySources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies
{
    public class ProxyPool
    {
        /// <summary>All the proxies currently in the pool.</summary>
        public IEnumerable<Proxy> Proxies => proxies;

        /// <summary>Checks if all proxies are banned.</summary>
        public bool AllBanned => proxies.All(p => p.ProxyStatus == ProxyStatus.Bad || p.ProxyStatus == ProxyStatus.Banned);

        private List<Proxy> proxies = new();
        private bool isReloadingProxies = false;
        private readonly List<ProxySource> sources;
        private readonly IEnumerable<ProxyType> allowedProxyTypes;

        /// <summary>
        /// Initializes the proxy pool given the proxy sources.
        /// </summary>
        public ProxyPool(IEnumerable<ProxySource> sources, ProxyType[] allowedProxyTypes = null)
        {
            this.sources = sources.ToList();
            this.allowedProxyTypes = allowedProxyTypes ?? new ProxyType[] { ProxyType.Http, ProxyType.Socks4, ProxyType.Socks5 };
        }

        /// <summary>
        /// Sets all the BANNED and BAD proxies status to AVAILABLE and resets their Uses.
        /// </summary>
        public void UnbanAll()
        {
            proxies.ForEach(p =>
            { 
                if (p.ProxyStatus == ProxyStatus.Banned || p.ProxyStatus == ProxyStatus.Bad)
                {
                    p.ProxyStatus = ProxyStatus.Available;
                    p.BeingUsedBy = 0;
                    p.TotalUses = 0;
                }
            });
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
            IEnumerable<Proxy> possibleProxies = proxies;

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

        /// <summary>
        /// Releases a proxy that was being used, optionally banning it.
        /// </summary>
        public void ReleaseProxy(Proxy proxy, bool ban = false)
        {
            proxy.TotalUses++;
            proxy.BeingUsedBy--;
            proxy.ProxyStatus = ban ? ProxyStatus.Banned : ProxyStatus.Available;
        }

        /// <summary>
        /// Removes duplicates.
        /// </summary>
        public void RemoveDuplicates()
            => proxies = proxies.Distinct(new GenericComparer<Proxy>()).ToList();

        /// <summary>
        /// Shuffles proxies.
        /// </summary>
        public void Shuffle()
            => proxies.Shuffle();

        /// <summary>
        /// Reloads all proxies in the pool from the provided sources.
        /// </summary>
        public async Task ReloadAll(bool shuffle = true)
        {
            if (isReloadingProxies)
            {
                return;
            }

            isReloadingProxies = true;

            var tasks = sources.Select(async source => 
            {
                try
                {
                    return await source.GetAll();
                }
                catch
                {
                    switch (source)
                    {
                        case FileProxySource x:
                            Console.WriteLine($"Could not reload proxies from source {x.FileName}");
                            break;

                        case RemoteProxySource x:
                            Console.WriteLine($"Could not reload proxies from source {x.Url}");
                            break;

                        default:
                            Console.WriteLine("Could not reload proxies from unknown source");
                            break;
                    }
                    
                    return new List<Proxy>();
                }
            });

            try
            {
                var results = await Task.WhenAll(tasks);
                proxies = results.SelectMany(r => r)
                    .Where(p => allowedProxyTypes.Contains(p.Type)) // Filter by allowed types
                    .ToList();

                if (shuffle)
                {
                    Shuffle();
                }
            }
            finally
            {
                isReloadingProxies = false;
            }
        }
    }
}
