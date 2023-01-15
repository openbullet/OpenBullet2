using RuriLib.Models.Hits.HitOutputs;

namespace OpenBullet2.Core.Models.Hits
{
    /// <summary>
    /// Options for a <see cref="CustomWebhookHitOutput"/>.
    /// </summary>
    public class MongoDBHitOutputOptions : HitOutputOptions
    {
        /// <summary>
        /// The URL of the remote webhook.
        /// </summary>
        public string ClusterURL { get; set; } = "Database Cluster URL";

        /// <summary>
        /// The username to send inside the body of the data, to identify who
        /// sent the data to the webhook.
        /// </summary>
        public string CollectionName { get; set; } = "Name of Collection";

        public string DatabaseName { get; set; } = "Name of Database";

        /// <summary>
        /// Whether to only send proper hits (SUCCESS status) to the webhook.
        /// </summary>
        public bool OnlyHits { get; set; } = true;
    }
}
