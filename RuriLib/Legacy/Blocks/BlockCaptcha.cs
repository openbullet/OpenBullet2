using Newtonsoft.Json;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using System;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that can solve captcha challenges.
    /// </summary>
    public abstract class BlockCaptcha : BlockBase
    {
        /// <summary>The balance of the account of the captcha-solving service.</summary>
        [JsonIgnore]
        public decimal Balance { get; set; } = 0;

        /// <inheritdoc />
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            var provider = data.Providers.Captcha;

            // If bypass balance check, skip this method.
            if (!provider.CheckBalanceBeforeSolving)
            {
                return;
            }

            // Get balance. If balance is under a certain threshold, don't ask for captcha solve
            Balance = 0; // Reset it or the block will save it for future calls
            data.Logger.Log("Checking balance...", LogColors.White);

            Balance = await provider.GetBalanceAsync();

            if (Balance <= 0)
            {
                throw new Exception($"[{provider.ServiceType}] Bad token/credentials or zero balance!");
            }

            data.Logger.Log($"[{provider.ServiceType}] Current Balance: ${Balance}", LogColors.GreenYellow);
            data.CaptchaCredit = Balance;
        }
    }
}
