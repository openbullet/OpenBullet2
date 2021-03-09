namespace OpenBullet2.Models.Hits
{
    public class TelegramBotHitOutputOptions : HitOutputOptions
    {
        public string Token { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
    }
}
