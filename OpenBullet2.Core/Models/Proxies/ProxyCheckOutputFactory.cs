using OpenBullet2.Core.Repositories;
using RuriLib.Models.Proxies;
using System;

namespace OpenBullet2.Core.Models.Proxies
{
    /// <summary>
    /// Factory that creates an <see cref="IProxyCheckOutput"/> from the <see cref="ProxyCheckOutputOptions"/>.
    /// </summary>
    public class ProxyCheckOutputFactory
    {
        private readonly IProxyRepository proxyRepo;

        public ProxyCheckOutputFactory(IProxyRepository proxyRepo)
        {
            this.proxyRepo = proxyRepo;
        }

        /// <summary>
        /// Creates an <see cref="IProxyCheckOutput"/> from the <see cref="ProxyCheckOutputOptions"/>.
        /// </summary>
        public IProxyCheckOutput FromOptions(ProxyCheckOutputOptions options)
        {
            IProxyCheckOutput output = options switch
            {
                DatabaseProxyCheckOutputOptions _ => new DatabaseProxyCheckOutput(proxyRepo),
                _ => throw new NotImplementedException()
            };

            return output;
        }
    }
}
