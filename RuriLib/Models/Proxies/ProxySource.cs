using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies
{
    public abstract class ProxySource
    {
        public ProxyType DefaultType { get; set; } = ProxyType.Http;
        public string DefaultUsername { get; set; } = string.Empty;
        public string DefaultPassword { get; set; } = string.Empty;
        public int UserId { get; set; } = 0;

        protected readonly Random random = new Random();

        public virtual Task<IEnumerable<Proxy>> GetAll()
            => throw new NotImplementedException();
    }
}
