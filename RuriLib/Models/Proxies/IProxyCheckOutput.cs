using System.Threading.Tasks;

namespace RuriLib.Models.Proxies
{
    public interface IProxyCheckOutput
    {
        Task StoreAsync(Proxy proxy);
    }
}
