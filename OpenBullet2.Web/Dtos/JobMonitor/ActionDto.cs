using OpenBullet2.Web.Attributes;

namespace OpenBullet2.Web.Dtos.JobMonitor;

/// <summary>
/// Generic action DTO.
/// </summary>
public class ActionDto : PolyDto
{

}

/// <summary>
/// Waits for a certain amount of time before executing
/// the next action in the chain.
/// </summary>
[PolyType("wait")]
public class WaitActionDto : ActionDto
{
    /// <summary>
    /// The amount of time to wait.
    /// </summary>
    public TimeSpan TimeSpan { get; set; }
}

/// <summary>
/// Sets the relative start condition of a job to the given time span.
/// </summary>
[PolyType("setRelativeStartCondition")]
public class SetRelativeStartConditionActionDto : ActionDto
{
    /// <summary>
    /// The id of the target job.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// The amount of time the target job should wait for
    /// before starting.
    /// </summary>
    public TimeSpan TimeSpan { get; set; }
}

/// <summary>
/// Stops the target job.
/// </summary>
[PolyType("stopJob")]
public class StopJobActionDto : ActionDto
{
    /// <summary>
    /// The id of the target job.
    /// </summary>
    public int JobId { get; set; }
}

/// <summary>
/// Aborts the target job.
/// </summary>
[PolyType("abortJob")]
public class AbortJobActionDto : ActionDto
{
    /// <summary>
    /// The id of the target job.
    /// </summary>
    public int JobId { get; set; }
}

/// <summary>
/// Starts the target job.
/// </summary>
[PolyType("startJob")]
public class StartJobActionDto : ActionDto
{
    /// <summary>
    /// The id of the target job.
    /// </summary>
    public int JobId { get; set; }
}

/// <summary>
/// Sends a message through a discord webhook.
/// </summary>
[PolyType("discordWebhook")]
public class DiscordWebhookActionDto : ActionDto
{
    /// <summary>
    /// The webhook link.
    /// </summary>
    public string Webhook { get; set; } = string.Empty;

    /// <summary>
    /// The message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Sends a message through a telegram bot.
/// </summary>
[PolyType("telegramBot")]
public class TelegramBotActionDto : ActionDto
{
    /// <summary>
    /// The API server.
    /// </summary>
    public string ApiServer { get; set; } = "https://api.telegram.org/";

    /// <summary>
    /// The authentication token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The id of the chat.
    /// </summary>
    public long ChatId { get; set; } = 0;

    /// <summary>
    /// The message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
