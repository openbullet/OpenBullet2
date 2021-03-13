using RuriLib.Extensions;
using RuriLib.Models.Proxies.ProxySources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private readonly ProxyPoolOptions options;

        /// <summary>
        /// Initializes the proxy pool given the proxy sources.
        /// </summary>
        public ProxyPool(IEnumerable<ProxySource> sources, ProxyPoolOptions options = null)
        {
            this.sources = sources.ToList();
            this.options = options ?? new ProxyPoolOptions();
        }

        /// <summary>
        /// Sets all the BANNED and BAD proxies status to AVAILABLE and resets their Uses.
        /// </summary>
        public void UnbanAll(TimeSpan minimumBanTime)
        {
            var now = DateTime.Now;
            proxies.Where(p => now > p.LastBanned + minimumBanTime).ToList().ForEach(p =>
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
        [MethodImpl(MethodImplOptions.AggressiveOptimization)] //hot path
        public Proxy GetProxy(bool evenBusy = false, int maxUses = 0)
        {

            for (int i = 0; i < proxies.Count; i++)
            {
                Proxy px = proxies[i];
                if (evenBusy)
                {
                    if (px.ProxyStatus == ProxyStatus.Available || px.ProxyStatus == ProxyStatus.Busy)
                    {
                        if (maxUses > 0)
                        {
                            if (px.TotalUses < maxUses)
                            {
                                px.BeingUsedBy++;
                                px.ProxyStatus = ProxyStatus.Busy;
                                return px;
                            }
                        }
                        else
                        {
                            px.BeingUsedBy++;
                            px.ProxyStatus = ProxyStatus.Busy;
                            return px;
                        }
                    }
                }
                else
                {
                    if (px.ProxyStatus == ProxyStatus.Available)
                    {
                        if (maxUses > 0)
                        {
                            if (px.TotalUses < maxUses)
                            {
                                px.BeingUsedBy++;
                                px.ProxyStatus = ProxyStatus.Busy;
                                return px;
                            }
                        }
                        else
                        {
                            px.BeingUsedBy++;
                            px.ProxyStatus = ProxyStatus.Busy;
                            return px;
                        }
                    }
                }

            }
            return default;
        }

        /// <summary>
        /// Releases a proxy that was being used, optionally banning it.
        /// </summary>
        public void ReleaseProxy(Proxy proxy, bool ban = false)
        {
            proxy.TotalUses++;
            proxy.BeingUsedBy--;

            if (ban)
            {
                proxy.ProxyStatus = ProxyStatus.Banned;
                proxy.LastBanned = DateTime.Now;
            }
            else
            {
                proxy.ProxyStatus = ProxyStatus.Available;
            }
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
                    .Where(p => options.AllowedTypes.Contains(p.Type)) // Filter by allowed types
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
