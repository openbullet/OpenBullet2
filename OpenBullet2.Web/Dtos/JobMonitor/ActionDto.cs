using OpenBullet2.Web.Attributes;
using RuriLib.Models.Jobs.Monitor.Actions;

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
[PolyType("waitAction")]
[MapsFrom(typeof(WaitAction), false)]
[MapsTo(typeof(WaitAction), false)]
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
[PolyType("setRelativeStartConditionAction")]
[MapsFrom(typeof(SetRelativeStartConditionAction), false)]
[MapsTo(typeof(SetRelativeStartConditionAction), false)]
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
[PolyType("stopJobAction")]
[MapsFrom(typeof(StopJobAction))]
[MapsTo(typeof(StopJobAction))]
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
[PolyType("abortJobAction")]
[MapsFrom(typeof(AbortJobAction))]
[MapsTo(typeof(AbortJobAction))]
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
[PolyType("startJobAction")]
[MapsFrom(typeof(StartJobAction))]
[MapsTo(typeof(StartJobAction))]
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
[PolyType("discordWebhookAction")]
[MapsFrom(typeof(DiscordWebhookAction))]
[MapsTo(typeof(DiscordWebhookAction))]
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
[PolyType("telegramBotAction")]
[MapsFrom(typeof(TelegramBotAction))]
[MapsTo(typeof(TelegramBotAction))]
public class TelegramBotActionDto : ActionDto
{
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
