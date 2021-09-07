namespace OpenBullet2.Core.Models.Jobs
{
    /// <summary>
    /// The available job types.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// Used to run a config using multiple bots.
        /// </summary>
        MultiRun,

        /// <summary>
        /// Used to check proxies.
        /// </summary>
        ProxyCheck,

        Spider,
        Ripper,
        PuppeteerUnitTest
    }
}
