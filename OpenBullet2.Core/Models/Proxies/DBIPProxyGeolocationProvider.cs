using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MaxMind.GeoIP2;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Core.Models.Proxies
{
    public class DBIPProxyGeolocationProvider : IProxyGeolocationProvider, IDisposable
    {
        private readonly DatabaseReader reader;

        public DBIPProxyGeolocationProvider(string dbFile)
        {
            reader = new DatabaseReader(dbFile);
        }

        public void Dispose() => reader.Dispose();

        public async Task<string> Geolocate(string host)
        {
            if (!IPAddress.TryParse(host, out var _))
            {
                var addresses = await Dns.GetHostAddressesAsync(host);
                
                if (addresses.Length > 0)
                {
                    host = addresses.First().MapToIPv4().ToString();
                }
            }

            return reader.Country(host).Country.Name;
        }
    }
}
