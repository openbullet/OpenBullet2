namespace OpenBullet2.Core.Entities
{
    /// <summary>
    /// This entity stores a record that matches a given config ID and wordlist ID
    /// to a checkpoint in the checking process, identified by the amount of data
    /// lines processed up to the point when it was last saved.
    /// </summary>
    public class RecordEntity : Entity
    {
        /// <summary>
        /// The ID of the config that was running.
        /// </summary>
        public string ConfigId { get; set; }

        /// <summary>
        /// The ID of the wordlist that was being used.
        /// </summary>
        public int WordlistId { get; set; }

        /// <summary>
        /// The amount of data lines processed until the last save.
        /// </summary>
        public int Checkpoint { get; set; }
    }
}
