using Newtonsoft.Json;
using RuriLib.Functions.Time;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Hits.HitOutputs;

/// <summary>
/// Sends hits to a custom webhook endpoint.
/// </summary>
public class CustomWebhookHitOutput : IHitOutput
{
    /// <summary>Gets or sets the webhook URL.</summary>
    public string Url { get; set; }
    /// <summary>Gets or sets the user name attached to payloads.</summary>
    public string User { get; set; }
    /// <summary>Gets or sets a value indicating whether only successful hits should be sent.</summary>
    public bool OnlyHits { get; set; }

    /// <summary>
    /// Creates a custom webhook hit output.
    /// </summary>
    /// <param name="url">The webhook URL.</param>
    /// <param name="user">The user name attached to payloads.</param>
    /// <param name="onlyHits">Whether only successful hits should be sent.</param>
    public CustomWebhookHitOutput(string url, string user, bool onlyHits = true)
    {
        Url = url;
        User = user;
        OnlyHits = onlyHits;
    }

    /// <inheritdoc />
    public async Task Store(Hit hit)
    {
        if (OnlyHits && hit.Type != "SUCCESS")
        {
            return;
        }

        var data = new CustomWebhookData
        {
            Data = hit.Data.Data,
            CapturedData = hit.CapturedDataString,
            ConfigName = hit.Config.Metadata.Name,
            ConfigAuthor = hit.Config.Metadata.Author,
            Timestamp = hit.Date.ToUnixTime(),
            Type = hit.Type,
            User = User
        };

        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Url,
            new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
    }
}
