using RuriLib.Models.Hits.HitOutputs;

namespace OpenBullet2.Core.Models.Hits
{
    /// <summary>
    /// Options for a <see cref="DiscordWebhookHitOutput"/>.
    /// </summary>
    public class DiscordWebhookHitOutputOptions : HitOutputOptions
    {
        /// <summary>
        /// The URL of the webhook.
        /// </summary>
        public string Webhook { get; set; } = string.Empty;

        /// <summary>
        /// The username to use when sending the message.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The URL of the avatar picture to use when sending the message.
        /// </summary>
        public string AvatarUrl { get; set; } = string.Empty;

        /// <summary>
        /// Whether to only send proper hits (SUCCESS status) to the webhook.
        /// </summary>
        public bool OnlyHits { get; set; } = true;
    }
}
