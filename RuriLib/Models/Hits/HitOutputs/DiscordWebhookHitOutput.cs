using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Hits.HitOutputs
{
    public class DiscordWebhookHitOutput : IHitOutput
    {
        public string Webhook { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public bool OnlyHits { get; set; }

        public DiscordWebhookHitOutput(string webhook, string username = "", string avatarUrl = "", bool onlyHits = true)
        {
            Webhook = webhook;
            Username = username;
            AvatarUrl = avatarUrl;
            OnlyHits = onlyHits;
        }

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
                obj.Add("username", JToken.FromObject(Username));

            if (!string.IsNullOrWhiteSpace(AvatarUrl))
                obj.Add("avatar_url", JToken.FromObject(AvatarUrl));

            await client.PostAsync(Webhook, 
                new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
        }
    }
}
