using System.Threading.Tasks;

namespace RuriLib.Models.Proxies
{
    public interface IProxyGeolocationProvider
    {
        Task<string> GeolocateAsync(string host);
    }
}
