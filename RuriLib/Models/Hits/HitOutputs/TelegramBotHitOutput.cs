using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Hits.HitOutputs;

/// <summary>
/// Sends hits through a Telegram bot.
/// </summary>
public class TelegramBotHitOutput : IHitOutput
{
    /// <summary>Gets or sets the bot token.</summary>
    public string Token { get; set; }
    /// <summary>Gets or sets the target chat identifier.</summary>
    public long ChatId { get; set; }
    /// <summary>Gets or sets a value indicating whether only successful hits should be sent.</summary>
    public bool OnlyHits { get; set; }

    /// <summary>
    /// Creates a Telegram bot hit output.
    /// </summary>
    /// <param name="token">The bot token.</param>
    /// <param name="chatId">The target chat identifier.</param>
    /// <param name="onlyHits">Whether only successful hits should be sent.</param>
    public TelegramBotHitOutput(string token, long chatId, bool onlyHits = true)
    {
        Token = token;
        ChatId = chatId;
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

        var webhook = $"https://api.telegram.org/bot{Token}/sendMessage";

        var obj = new Dictionary<string, object>
        {
            { "chat_id", ChatId },
            { "text", hit.ToString() }
        };

        await client.PostAsync(webhook,
            new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
    }
}
