using Newtonsoft.Json.Linq;
using RuriLib.Providers.UserAgents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenBullet2.Core.Services
{
    /// <summary>
    /// Random UA provider that uses the User-Agents collected by intoli.com
    /// </summary>
    public class IntoliRandomUAProvider : IRandomUAProvider
    {
        private readonly Dictionary<UAPlatform, UserAgent[]> distributions = new Dictionary<UAPlatform, UserAgent[]>();
        private readonly Random rand;

        /// <inheritdoc/>
        public int Total => distributions[UAPlatform.All].Length;

        public IntoliRandomUAProvider(string jsonFile)
        {
            var json = File.ReadAllText(jsonFile);
            var array = JArray.Parse(json);

            var agents = new List<UserAgent>();
            foreach (var elem in array)
            {
                agents.Add(new UserAgent(elem.Value<string>("userAgent"),
                    ConvertPlatform(elem.Value<string>("platform")), elem.Value<double>("weight"), 0));
            }
            
            rand = new Random();

            if (agents.Count == 0)
                throw new Exception("No valid user agents found in user-agents.json");

            foreach (var platform in (UAPlatform[])Enum.GetValues(typeof(UAPlatform)))
                distributions[platform] = ComputeDistribution(agents, platform);
        }

        /// <inheritdoc/>
        public string Generate() => Generate(UAPlatform.All);

        /// <inheritdoc/>
        public string Generate(UAPlatform platform)
        {
            // Take the correct precomputed cumulative distribution
            var distribution = distributions[platform];

            // Take the maximum value of the cumulative function
            var max = distribution.Last().cumulative;

            // Generate a random double up to the previously computed maximum
            var random = rand.NextDouble() * max;

            // Return the first user agent with cumulative greater or equal to the random one
            return distribution.First(u => u.cumulative >= random).userAgentString;
        }

        private static UserAgent[] ComputeDistribution(IEnumerable<UserAgent> agents, UAPlatform platform)
        {
            var valid = agents.Where(a => BelongsToPlatform(a.platform, platform));

            var distribution = new List<UserAgent>();
            double cumulative = 0;
            foreach (var elem in valid)
            {
                cumulative += elem.weight;
                distribution.Add(new UserAgent(elem.userAgentString, elem.platform, elem.weight, cumulative));
            }

            return distribution.ToArray();
        }

        private static UAPlatform ConvertPlatform(string platform) => platform switch
        {
            "iPad" => UAPlatform.iPad,
            "iPhone" => UAPlatform.iPhone,
            "Linux aarch64" => UAPlatform.Android,
            "Linux armv71" => UAPlatform.Android,
            "Linux armv81" => UAPlatform.Android,
            "Linux x86_64" => UAPlatform.Linux,
            "MacIntel" => UAPlatform.Mac,
            "Win32" => UAPlatform.Windows,
            "Win64" => UAPlatform.Windows,
            "Windows" => UAPlatform.Windows,
            _ => UAPlatform.Windows
        };

        private static bool BelongsToPlatform(UAPlatform current, UAPlatform required) => required switch
        {
            UAPlatform.All => true,
            UAPlatform.Desktop => current == UAPlatform.Linux || current == UAPlatform.Mac || current == UAPlatform.Windows,
            UAPlatform.Mobile => current == UAPlatform.iPhone || current == UAPlatform.iPad || current == UAPlatform.Android,
            _ => current == required
        };
    }
}
