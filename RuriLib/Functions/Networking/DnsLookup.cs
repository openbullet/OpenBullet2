using Newtonsoft.Json.Linq;
using RuriLib.Functions.Http;
using RuriLib.Http.Models;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Functions.Networking
{
    public static class DnsLookup
    {
        /// <summary>
        /// Retrieves a list of records from Google's DNS over HTTP service at dns.google.com.
        /// The list is ordered by priority.
        /// </summary>
        public static async Task<List<string>> FromGoogle(string domain, string type, Proxy proxy = null,
            int timeout = 30000, CancellationToken cancellationToken = default)
        {
            var url = $"https://dns.google.com/resolve?name={Uri.EscapeDataString(domain)}&type={type}";
            
            using var httpClient = HttpFactory.GetRLHttpClient(proxy, new() 
            {
                ConnectTimeout = TimeSpan.FromMilliseconds(timeout), 
                ReadWriteTimeout = TimeSpan.FromMilliseconds(timeout)
            });

            using var request = new HttpRequest
            {
                Uri = new Uri(url),
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var obj = JObject.Parse(json);

            return obj["Answer"]
                .Select(i => i.Value<string>("data"))
                .Select(i => (int.Parse(i.Split(' ')[0]), i.Split(' ')[1]))
                .OrderBy(kvp => kvp.Item1)
                .Select(kvp => kvp.Item2.TrimEnd('.'))
                .ToList();
        }
    }
}
