using RuriLib.Models.Hits.HitOutputs;

namespace OpenBullet2.Core.Models.Hits
{
    /// <summary>
    /// Options for a <see cref="CustomWebhookHitOutput"/>.
    /// </summary>
    public class CustomWebhookHitOutputOptions : HitOutputOptions
    {
        /// <summary>
        /// The URL of the remote webhook.
        /// </summary>
        public string Url { get; set; } = "http://mycustomwebhook.com";

        /// <summary>
        /// The username to send inside the body of the data, to identify who
        /// sent the data to the webhook.
        /// </summary>
        public string User { get; set; } = "Anonymous";

        /// <summary>
        /// Whether to only send proper hits (SUCCESS status) to the webhook.
        /// </summary>
        public bool OnlyHits { get; set; } = true;
    }
}
