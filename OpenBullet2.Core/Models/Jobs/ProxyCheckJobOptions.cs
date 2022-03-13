using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Models.Settings;
using RuriLib.Models.Jobs;

namespace OpenBullet2.Core.Models.Jobs
{
    /// <summary>
    /// Options for a <see cref="ProxyCheckJob"/>.
    /// </summary>
    public class ProxyCheckJobOptions : JobOptions
    {
        /// <summary>
        /// The amount of bots that will check the proxies concurrently.
        /// </summary>
        public int Bots { get; set; } = 1;

        /// <summary>
        /// The ID of the proxy group to check.
        /// </summary>
        public int GroupId { get; set; } = -1;

        /// <summary>
        /// Whether to only check the proxies that were never been tested.
        /// </summary>
        public bool CheckOnlyUntested { get; set; } = true;

        /// <summary>
        /// The target site against which proxies should be checked.
        /// </summary>
        public ProxyCheckTarget Target { get; set; } = null;

        /// <summary>
        /// The maximum timeout that a valid proxy should have, in milliseconds.
        /// </summary>
        public int TimeoutMilliseconds { get; set; } = 10000;

        /// <summary>
        /// The options for the output of a proxy check.
        /// </summary>
        public ProxyCheckOutputOptions CheckOutput { get; set; }
    }
}
