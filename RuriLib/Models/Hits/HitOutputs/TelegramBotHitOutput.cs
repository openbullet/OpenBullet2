using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Hits.HitOutputs
{
    public class TelegramBotHitOutput : IHitOutput
    {
        public string ApiServer { get; set; }
        public string Token { get; set; }
        public long ChatId { get; set; }

        public TelegramBotHitOutput(string apiServer, string token, long chatId)
        {
            ApiServer = apiServer;
            Token = token;
            ChatId = chatId;
        }

        public async Task Store(Hit hit)
        {
            using var client = new HttpClient();

            var webhook = $"{new Uri(ApiServer)}bot{Token}/sendMessage";

            var obj = new Dictionary<string, object>()
            {
                { "chat_id", ChatId },
                { "text", hit.ToString() }
            };

            await client.PostAsync(webhook,
                new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
        }
    }
}
