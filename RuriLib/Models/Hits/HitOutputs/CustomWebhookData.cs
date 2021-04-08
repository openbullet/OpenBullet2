using Newtonsoft.Json;

namespace RuriLib.Models.Hits.HitOutputs
{
    public class CustomWebhookData
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("capturedData")]
        public string CapturedData { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("configName")]
        public string ConfigName { get; set; }

        [JsonProperty("configAuthor")]
        public string ConfigAuthor { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
    }
}
