using OpenBullet2.Core.Services;
using RuriLib.Models.Hits;
using RuriLib.Models.Hits.HitOutputs;
using System;

namespace OpenBullet2.Core.Models.Hits
{
    /// <summary>
    /// A factory that creates an <see cref="IHitOutput"/> from <see cref="HitOutputOptions"/>.
    /// </summary>
    public class HitOutputFactory
    {
        private readonly HitStorageService hitStorage;

        public HitOutputFactory(HitStorageService hitStorage)
        {
            this.hitStorage = hitStorage;
        }

        /// <summary>
        /// Creates an <see cref="IHitOutput"/> from <see cref="HitOutputOptions"/>.
        /// </summary>
        public IHitOutput FromOptions(HitOutputOptions options)
        {
            IHitOutput output = options switch
            {
                DatabaseHitOutputOptions _ => new DatabaseHitOutput(hitStorage),
                FileSystemHitOutputOptions x => new FileSystemHitOutput(x.BaseDir),
                DiscordWebhookHitOutputOptions x => new DiscordWebhookHitOutput(x.Webhook, x.Username, x.AvatarUrl),
                TelegramBotHitOutputOptions x => new TelegramBotHitOutput(x.ApiServer, x.Token, x.ChatId),
                CustomWebhookHitOutputOptions x => new CustomWebhookHitOutput(x.Url, x.User),
                _ => throw new NotImplementedException()
            };

            return output;
        }
    }
}
