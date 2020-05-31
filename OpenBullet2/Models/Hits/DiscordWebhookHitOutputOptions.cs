namespace OpenBullet2.Models.Hits
{
    public class DiscordWebhookHitOutputOptions : HitOutputOptions
    {
        public string Webhook { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
