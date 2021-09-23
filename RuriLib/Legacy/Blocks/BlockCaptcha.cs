using Newtonsoft.Json;
using RuriLib.Functions.Captchas;
using System;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that can solve captcha challenges.
    /// </summary>
    [Obsolete]
    public abstract class BlockCaptcha : BlockBase
    {
        /// <summary>The balance of the account of the captcha-solving service.</summary>
        [JsonIgnore]
        public decimal Balance { get; set; } = 0;

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            // If bypass balance check, skip this method.
            if (data.GlobalSettings.Captchas.BypassBalanceCheck) return;

            // Get balance. If balance is under a certain threshold, don't ask for captcha solve
            Balance = 0; // Reset it or the block will save it for future calls
            data.Log(new LogEntry("Checking balance...", Colors.White));

            Balance = Captchas.GetService(data.GlobalSettings.Captchas).GetBalanceAsync().Result;

            if (Balance <= 0) throw new System.Exception($"[{data.GlobalSettings.Captchas.CurrentService}] Bad token/credentials or zero balance!");

            data.Log(new LogEntry($"[{data.GlobalSettings.Captchas.CurrentService}] Current Balance: ${Balance}", Colors.GreenYellow));
            data.Balance = Balance;
        }
    }
}
