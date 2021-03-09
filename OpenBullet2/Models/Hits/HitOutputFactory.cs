using OpenBullet2.Repositories;
using RuriLib.Models.Hits;
using RuriLib.Models.Hits.HitOutputs;
using System;

namespace OpenBullet2.Models.Hits
{
    public class HitOutputFactory
    {
        private readonly IHitRepository hitRepo;

        public HitOutputFactory(IHitRepository hitRepo)
        {
            this.hitRepo = hitRepo;
        }

        public IHitOutput FromOptions(HitOutputOptions options)
        {
            IHitOutput output = options switch
            {
                DatabaseHitOutputOptions _ => new DatabaseHitOutput(hitRepo),
                FileSystemHitOutputOptions x => new FileSystemHitOutput(x.BaseDir),
                DiscordWebhookHitOutputOptions x => new DiscordWebhookHitOutput(x.Webhook, x.Username, x.AvatarUrl),
                TelegramBotHitOutputOptions x => new TelegramBotHitOutput(x.ApiServer, x.Token, x.ChatId),
                _ => throw new NotImplementedException()
            };

            return output;
        }
    }
}
