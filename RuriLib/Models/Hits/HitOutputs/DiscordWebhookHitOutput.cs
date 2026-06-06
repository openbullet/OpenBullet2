using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Hits.HitOutputs;

/// <summary>
/// Sends hits to a Discord webhook.
/// </summary>
public class DiscordWebhookHitOutput : IHitOutput
{
    /// <summary>Gets or sets the webhook URL.</summary>
    public string Webhook { get; set; }
    /// <summary>Gets or sets the override username.</summary>
    public string Username { get; set; }
    /// <summary>Gets or sets the avatar URL.</summary>
    public string AvatarUrl { get; set; }
    /// <summary>Gets or sets a value indicating whether only successful hits should be sent.</summary>
    public bool OnlyHits { get; set; }

    /// <summary>
    /// Creates a Discord webhook hit output.
    /// </summary>
    /// <param name="webhook">The webhook URL.</param>
    /// <param name="username">The optional override username.</param>
    /// <param name="avatarUrl">The optional avatar URL.</param>
    /// <param name="onlyHits">Whether only successful hits should be sent.</param>
    public DiscordWebhookHitOutput(string webhook, string username = "", string avatarUrl = "", bool onlyHits = true)
    {
        Webhook = webhook;
        Username = username;
        AvatarUrl = avatarUrl;
        OnlyHits = onlyHits;
    }

    /// <inheritdoc />
    public async Task Store(Hit hit)
    {
        if (OnlyHits && hit.Type != "SUCCESS")
        {
            return;
        }

        using var client = new HttpClient();

        var obj = new JObject
        {
            { "content", JToken.FromObject(hit.ToString()) }
        };

        if (!string.IsNullOrWhiteSpace(Username))
        {
            obj.Add("username", JToken.FromObject(Username));
        }

        if (!string.IsNullOrWhiteSpace(AvatarUrl))
        {
            obj.Add("avatar_url", JToken.FromObject(AvatarUrl));
        }

        await client.PostAsync(Webhook,
            new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
    }
}
