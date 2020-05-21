using System;

namespace OpenBullet2.Entities
{
    public class HitEntity : Entity
    {
        public string Data { get; set; } // As JSON
        public string CapturedData { get; set; } // As JSON
        public string Proxy { get; set; } // Not the ID because it might be deleted
        public DateTime Date { get; set; }
        public string Type { get; set; } // The hit type like SUCCESS, CUSTOM, NONE
        public string ConfigId { get; set; }
        public string ConfigName { get; set; } // Needed to still identify the config name even if the config was deleted
        public string ConfigCategory { get; set; } // Needed to still identify the category even if it was deleted
        public string WordlistId { get; set; }
        public string WordlistName { get; set; } // Needed to still identify the wordlist name even if the wordlist was deleted
    }
}
