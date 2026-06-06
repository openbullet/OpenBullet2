using Newtonsoft.Json;

namespace RuriLib.Models.Hits.HitOutputs;

/// <summary>
/// Payload sent to a custom webhook hit output.
/// </summary>
public class CustomWebhookData
{
    /// <summary>Gets or sets the source data.</summary>
    [JsonProperty("data")]
    public string Data { get; set; } = string.Empty;

    /// <summary>Gets or sets the hit type.</summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the captured data string.</summary>
    [JsonProperty("capturedData")]
    public string CapturedData { get; set; } = string.Empty;

    /// <summary>Gets or sets the Unix timestamp.</summary>
    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>Gets or sets the config name.</summary>
    [JsonProperty("configName")]
    public string ConfigName { get; set; } = string.Empty;

    /// <summary>Gets or sets the config author.</summary>
    [JsonProperty("configAuthor")]
    public string ConfigAuthor { get; set; } = string.Empty;

    /// <summary>Gets or sets the user name.</summary>
    [JsonProperty("user")]
    public string User { get; set; } = string.Empty;
}
