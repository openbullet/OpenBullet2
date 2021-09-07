using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MaxMind.GeoIP2;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Core.Models.Proxies
{
    /// <summary>
    /// A provider that uses the free database from https://www.maxmind.com/ to geolocate proxies by IP.
    /// </summary>
    public class DBIPProxyGeolocationProvider : IProxyGeolocationProvider, IDisposable
    {
        private readonly DatabaseReader reader;

        public DBIPProxyGeolocationProvider(string dbFile)
        {
            reader = new DatabaseReader(dbFile);
        }

        /// <inheritdoc/>
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

        public void Dispose()
        {
            reader.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
