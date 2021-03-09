using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Hits.HitOutputs
{
    public class TelegramBotHitOutput : IHitOutput
    {
        public string Token { get; set; }
        public string ChatId { get; set; }

        public TelegramBotHitOutput(string token = "", string chatId = "")
        {
            Token = token;
            ChatId = chatId;
        }

        public async Task Store(Hit hit)
        {
            using var client = new HttpClient();
            var webhook = $"https://api.telegram.org/bot{Token}/sendMessage";
            var obj = new JObject
            {
                { "chat_id", JToken.FromObject(ChatId) },
                { "text", JToken.FromObject(hit.ToString()) }
            };

            await client.PostAsync(webhook, 
                new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
        }
    }
}
