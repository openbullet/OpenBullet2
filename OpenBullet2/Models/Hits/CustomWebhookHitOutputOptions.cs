namespace OpenBullet2.Models.Hits
{
    public class CustomWebhookHitOutputOptions : HitOutputOptions
    {
        public string Url { get; set; } = "http://mycustomwebhook.com";
        public string User { get; set; } = "Anonymous";
    }
}
