namespace OpenBullet2.Models.Hits
{
    public class TelegramBotHitOutputOptions : HitOutputOptions
    {
        public string ApiServer { get; set; } = "https://api.telegram.org/";
        public string Token { get; set; } = string.Empty;
        public long ChatId { get; set; } = 0;
    }
}
