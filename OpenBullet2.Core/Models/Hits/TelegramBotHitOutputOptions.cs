using RuriLib.Models.Hits.HitOutputs;

namespace OpenBullet2.Core.Models.Hits
{
    /// <summary>
    /// Options for a <see cref="TelegramBotHitOutput"/>.
    /// </summary>
    public class TelegramBotHitOutputOptions : HitOutputOptions
    {
        /// <summary>
        /// The API server URL.
        /// </summary>
        public string ApiServer { get; set; } = "https://api.telegram.org/";

        /// <summary>
        /// The authentication token.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the telegram chat.
        /// </summary>
        public long ChatId { get; set; } = 0;

        /// <summary>
        /// Whether to only send proper hits (SUCCESS status) to the webhook.
        /// </summary>
        public bool OnlyHits { get; set; } = true;
    }
}
