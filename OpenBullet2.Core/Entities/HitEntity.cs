using System;

namespace OpenBullet2.Core.Entities
{
    /// <summary>
    /// This entity stores a hit from a job in the database.
    /// </summary>
    public class HitEntity : Entity
    {
        /// <summary>
        /// The data that was provided to the bot to get the hit, as a json string.
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// The variables captured by the bot, as a json string.
        /// </summary>
        public string CapturedData { get; set; } = string.Empty;

        /// <summary>
        /// The string representation of the proxy that was used to get the hit (blank if none).
        /// </summary>
        public string Proxy { get; set; } = string.Empty;

        /// <summary>
        /// The exact date when the hit was found.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The type of hit, for example SUCCESS, NONE, CUSTOM etc.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The ID of the owner of this hit (0 if admin).
        /// </summary>
        public int OwnerId { get; set; } = 0;

        /// <summary>
        /// The ID of the config that was used to get the hit.
        /// </summary>
        public string ConfigId { get; set; } = null;

        /// <summary>
        /// The name of the config that was used to get the hit.
        /// Needed to identify the name even if the config was deleted.
        /// </summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>
        /// The category of the config that was used to get the hit.
        /// Needed to identify the category even if the config was deleted.
        /// </summary>
        public string ConfigCategory { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the wordlist that was used to get the hit, -1 if no wordlist was used, < -1 for other data pools.
        /// </summary>
        public int WordlistId { get; set; } = -1;

        /// <summary>
        /// The name of the wordlist that was used to get the hit, blank if no wordlist was used.
        /// Needed to identify the name even if the wordlist was deleted. If <see cref="WordlistId"/> is less than -1,
        /// this field contains information about the data pool that was used.
        /// </summary>
        public string WordlistName { get; set; } = string.Empty;

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
