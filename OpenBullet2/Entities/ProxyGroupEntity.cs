namespace OpenBullet2.Entities
{
    public class ProxyGroupEntity : Entity
    {
        public string Name { get; set; }

        public GuestEntity Owner { get; set; } // The owner of the proxy group (null if admin)
    }
}
