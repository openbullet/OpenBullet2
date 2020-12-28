namespace OpenBullet2.Entities
{
    public class WordlistEntity : Entity
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Purpose { get; set; }
        public int Total { get; set; }
        public string Type { get; set; }

        public GuestEntity Owner { get; set; } // The owner of the wordlist (null if admin)
    }
}
