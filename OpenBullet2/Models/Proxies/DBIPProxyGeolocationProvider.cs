using System;
using System.Threading.Tasks;
using MaxMind.GeoIP2;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Models.Proxies
{
    public class DBIPProxyGeolocationProvider : IProxyGeolocationProvider, IDisposable
    {
        private readonly DatabaseReader reader;

        public DBIPProxyGeolocationProvider(string dbFile)
        {
            reader = new DatabaseReader(dbFile);
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public Task<string> Geolocate(string ip)
        {
            return Task.FromResult(reader.Country(ip).Country.Name);
        }
    }
}
