using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RuriLib.Models.Jobs.StartConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs.Monitor.Actions;

/// <summary>
/// Represents an action executed by the job monitor.
/// </summary>
public abstract class Action
{
    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="currentJobId">The identifier of the current job.</param>
    /// <param name="jobs">The available jobs.</param>
    /// <returns>A task that completes when the action finishes.</returns>
    public virtual Task Execute(int currentJobId, IEnumerable<Job> jobs)
        => throw new NotImplementedException();
}

// Waits a given amount of time
/// <summary>
/// Delays execution for a configured amount of time.
/// </summary>
public class WaitAction : Action
{
    /// <summary>Gets or sets the seconds component.</summary>
    public int Seconds { get; set; }
    /// <summary>Gets or sets the minutes component.</summary>
    public int Minutes { get; set; }
    /// <summary>Gets or sets the hours component.</summary>
    public int Hours { get; set; }
    /// <summary>Gets or sets the days component.</summary>
    public int Days { get; set; }

    /// <inheritdoc />
    public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
    {
        var toWait = Seconds + Minutes * 60 + Hours * 60 * 60 + Days * 24 * 60 * 60;
        return Task.Delay(toWait * 1000);
    }
}

// Sets the relative start condition of a job to the given timespan
/// <summary>
/// Sets a relative start condition on a target job.
/// </summary>
public class SetRelativeStartConditionAction : Action
{
    /// <summary>Gets or sets the target job identifier.</summary>
    public int JobId { get; set; }
    /// <summary>Gets or sets the seconds component.</summary>
    public int Seconds { get; set; }
    /// <summary>Gets or sets the minutes component.</summary>
    public int Minutes { get; set; }
    /// <summary>Gets or sets the hours component.</summary>
    public int Hours { get; set; }
    /// <summary>Gets or sets the days component.</summary>
    public int Days { get; set; }

    /// <inheritdoc />
    public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
    {
        jobs.First(j => j.Id == JobId).StartCondition =
            new RelativeTimeStartCondition { StartAfter = new TimeSpan(Days, Hours, Minutes, Seconds) };

        return Task.CompletedTask;
    }
}

// Stops the job with the given id
/// <summary>
/// Stops a target job.
/// </summary>
public class StopJobAction : Action
{
    /// <summary>Gets or sets the target job identifier.</summary>
    public int JobId { get; set; }

    /// <inheritdoc />
    public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
        => jobs.First(j => j.Id == JobId).Stop();
}

// Aborts the job with the given id
/// <summary>
/// Aborts a target job.
/// </summary>
public class AbortJobAction : Action
{
    /// <summary>Gets or sets the target job identifier.</summary>
    public int JobId { get; set; }

    /// <inheritdoc />
    public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
        => jobs.First(j => j.Id == JobId).Abort();
}

// Starts the job with the given id
/// <summary>
/// Starts a target job.
/// </summary>
public class StartJobAction : Action
{
    /// <summary>Gets or sets the target job identifier.</summary>
    public int JobId { get; set; }

    /// <inheritdoc />
    public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
        => jobs.First(j => j.Id == JobId).Start();
}

// Sends a message through a discord webhook
/// <summary>
/// Sends a message through a Discord webhook.
/// </summary>
public class DiscordWebhookAction : Action
{
    /// <summary>Gets or sets the webhook URL.</summary>
    public string Webhook { get; set; } = string.Empty;
    /// <summary>Gets or sets the message content.</summary>
    public string Message { get; set; } = string.Empty;

    /// <inheritdoc />
    public override async Task Execute(int currentJob, IEnumerable<Job> jobs)
    {
        using var client = new HttpClient();

        var obj = new JObject
        {
            { "content", JToken.FromObject(Message) }
        };

        await client.PostAsync(Webhook,
            new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
    }
}

// Sends a message through a telegram bot
/// <summary>
/// Sends a message through a Telegram bot.
/// </summary>
public class TelegramBotAction : Action
{
    /// <summary>Gets or sets the bot token.</summary>
    public string Token { get; set; } = string.Empty;
    /// <summary>Gets or sets the target chat identifier.</summary>
    public long ChatId { get; set; }
    /// <summary>Gets or sets the message content.</summary>
    public string Message { get; set; } = string.Empty;

    /// <inheritdoc />
    public override async Task Execute(int currentJob, IEnumerable<Job> jobs)
    {
        using var client = new HttpClient();

        var webhook = $"https://api.telegram.org/bot{Token}/sendMessage";

        var obj = new Dictionary<string, object>
        {
            { "chat_id", ChatId },
            { "text", Message }
        };

        await client.PostAsync(webhook,
            new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
    }
}
