using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Web.Attributes;

namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Information about an output where hits can be saved.
/// </summary>
public class HitOutputOptionsDto : PolyDto
{
}

/// <summary>
/// Saves hits to the database.
/// </summary>
[PolyType("databaseHitOutput")]
[MapsFrom(typeof(DatabaseHitOutputOptions))]
[MapsTo(typeof(DatabaseHitOutputOptions))]
public class DatabaseHitOutputOptionsDto : HitOutputOptionsDto
{
}

/// <summary>
/// Saves hits to the filesystem.
/// </summary>
[PolyType("fileSystemHitOutput")]
[MapsFrom(typeof(FileSystemHitOutputOptions))]
[MapsTo(typeof(FileSystemHitOutputOptions))]
public class FileSystemHitOutputOptionsDto : HitOutputOptionsDto
{
    /// <summary>
    /// The parent directory inside which the text files will be created.
    /// </summary>
    public string BaseDir { get; set; } = "Hits";
}

/// <summary>
/// Sends hits to a discord webhook.
/// </summary>
[PolyType("discordWebhookHitOutput")]
[MapsFrom(typeof(DiscordWebhookHitOutputOptions))]
[MapsTo(typeof(DiscordWebhookHitOutputOptions))]
public class DiscordWebhookHitOutputOptionsDto : HitOutputOptionsDto
{
    /// <summary>
    /// The URL of the webhook.
    /// </summary>
    public string Webhook { get; set; } = string.Empty;

    /// <summary>
    /// The username to use when sending the message.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The URL of the avatar picture to use when sending the message.
    /// </summary>
    public string AvatarUrl { get; set; } = string.Empty;

    /// <summary>
    /// Whether to only send proper hits (SUCCESS status) to the webhook.
    /// </summary>
    public bool OnlyHits { get; set; } = true;
}

/// <summary>
/// Sends hits to a telegram bot.
/// </summary>
[PolyType("telegramBotHitOutput")]
[MapsFrom(typeof(TelegramBotHitOutputOptions))]
[MapsTo(typeof(TelegramBotHitOutputOptions))]
public class TelegramBotHitOutputOptionsDto : HitOutputOptionsDto
{
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

/// <summary>
/// Sends hits to a custom webhook.
/// </summary>
[PolyType("customWebhookHitOutput")]
[MapsFrom(typeof(CustomWebhookHitOutputOptions))]
[MapsTo(typeof(CustomWebhookHitOutputOptions))]
public class CustomWebhookHitOutputOptionsDto : HitOutputOptionsDto
{
    /// <summary>
    /// The URL of the remote webhook.
    /// </summary>
    public string Url { get; set; } = "http://mycustomwebhook.com";

    /// <summary>
    /// The username to send inside the body of the data, to identify who
    /// sent the data to the webhook.
    /// </summary>
    public string User { get; set; } = "Anonymous";

    /// <summary>
    /// Whether to only send proper hits (SUCCESS status) to the webhook.
    /// </summary>
    public bool OnlyHits { get; set; } = true;
}
