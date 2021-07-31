namespace OpenBullet2.Core.Entities
{
    /// <summary>
    /// This entity stores a group that identifies a collection of proxies.
    /// </summary>
    public class ProxyGroupEntity : Entity
    {
        /// <summary>
        /// The name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The owner of this group (null if admin).
        /// </summary>
        public GuestEntity Owner { get; set; }
    }
}
