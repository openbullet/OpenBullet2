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

        public int OwnerId { get; set; } // The owner of the hit (0 if admin)

        // Config-related fields
        public string ConfigId { get; set; }
        public string ConfigName { get; set; } // Needed to still identify the config name even if the config was deleted
        public string ConfigCategory { get; set; } // Needed to still identify the category even if it was deleted
        
        // Wordlist-related fields
        public int WordlistId { get; set; }
        public string WordlistName { get; set; } // Needed to still identify the wordlist name even if the wordlist was deleted

        /// <summary>
        /// Gets a unique hash of the hit.
        /// </summary>
        /// <param name="ignoreWordlistName">Whether the wordlist name should affect the generated hash</param>
        public int GetHashCode(bool ignoreWordlistName = true)
        {
            var id = ignoreWordlistName
                ? Data + ConfigName
                : Data + ConfigName + WordlistName;
            
            return id.GetHashCode();
        }
    }
}
