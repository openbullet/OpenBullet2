namespace OpenBullet2.Core.Entities
{
    /// <summary>
    /// This entity stores the metadata of a wordlist in OpenBullet 2.
    /// </summary>
    public class WordlistEntity : Entity
    {
        /// <summary>
        /// The name of the wordlist.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The path to the file on disk that contains the lines of the wordlist.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The purpose of the wordlist.
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// The total amount of lines of the wordlist, usually calculated during import.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// The Wordlist Type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The owner of the wordlist (null if admin).
        /// </summary>
        public GuestEntity Owner { get; set; }
    }
}
