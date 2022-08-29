using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RuriLib.Models.Jobs.StartConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs.Monitor.Actions
{
    public abstract class Action
    {
        public virtual Task Execute(int currentJobId, IEnumerable<Job> jobs)
            => throw new NotImplementedException();
    }

    // Waits a given amount of time
    public class WaitAction : Action
    {
        public int Seconds { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Hours { get; set; } = 0;
        public int Days { get; set; } = 0;

        public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
        {
            int toWait = Seconds + Minutes * 60 + Hours * 60 * 60 + Days * 24 * 60 * 60;
            return Task.Delay(toWait * 1000);
        }
    }

    // Sets the relative start condition of a job to the given timespan
    public class SetRelativeStartConditionAction : Action
    {
        public int JobId { get; set; }
        public int Seconds { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Hours { get; set; } = 0;
        public int Days { get; set; } = 0;

        public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
        {
            jobs.First(j => j.Id == JobId).StartCondition = 
                new RelativeTimeStartCondition { StartAfter = new System.TimeSpan(Days, Hours, Minutes, Seconds) };

            return Task.CompletedTask;
        }
    }

    // Stops the job with the given id
    public class StopJobAction : Action
    {
        public int JobId { get; set; }

        public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
            => jobs.First(j => j.Id == JobId).Stop();
    }

    // Aborts the job with the given id
    public class AbortJobAction : Action
    {
        public int JobId { get; set; }

        public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
            => jobs.First(j => j.Id == JobId).Abort();
    }

    // Starts the job with the given id
    public class StartJobAction : Action
    {
        public int JobId { get; set; }

        public override Task Execute(int currentJobId, IEnumerable<Job> jobs)
            => jobs.First(j => j.Id == JobId).Start();
    }

    // Sends a message through a discord webhook
    public class DiscordWebhookAction : Action
    {
        public string Webhook { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

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
    public class TelegramBotAction : Action
    {
        public string ApiServer { get; set; } = "https://api.telegram.org/";
        public string Token { get; set; } = string.Empty;
        public long ChatId { get; set; } = 0;
        public string Message { get; set; } = string.Empty;

        public async override Task Execute(int currentJob, IEnumerable<Job> jobs)
        {
            using var client = new HttpClient();

            var webhook = $"{new Uri(ApiServer)}bot{Token}/sendMessage";

            var obj = new Dictionary<string, object>()
            {
                { "chat_id", ChatId },
                { "text", Message }
            };

            await client.PostAsync(webhook, 
                new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
        }
    }
}
