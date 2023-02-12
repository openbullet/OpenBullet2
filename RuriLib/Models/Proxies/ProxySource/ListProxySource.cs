using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies.ProxySources
{
    public class ListProxySource : ProxySource
    {
        private Proxy[] proxies;

        public ListProxySource(IEnumerable<Proxy> proxies)
        {
            this.proxies = proxies.ToArray();
        }

        public override Task<IEnumerable<Proxy>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult((IEnumerable<Proxy>)proxies);
    }
}
