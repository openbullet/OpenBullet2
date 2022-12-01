using RuriLib.Functions.Captchas;

namespace RuriLib.Models.Settings
{
    public class CaptchaSettings
    {
        public CaptchaServiceType CurrentService { get; set; } = CaptchaServiceType.TwoCaptcha;
        public int TimeoutSeconds { get; set; } = 120;
        public int PollingIntervalMilliseconds { get; set; } = 5000;
        public bool CheckBalanceBeforeSolving { get; set; } = true;

        public string AntiCaptchaApiKey { get; set; } = "";
        public string AZCaptchaApiKey { get; set; } = "";
        public string CapMonsterHost { get; set; } = "127.0.0.3";
        public int CapMonsterPort { get; set; } = 80;
        public string CaptchasDotIoApiKey { get; set; } = "";
        public string CustomTwoCaptchaApiKey { get; set; } = "";
        public string CustomTwoCaptchaDomain { get; set; } = "";
        public bool CustomTwoCaptchaOverrideHostHeader { get; set; } = true;
        public int CustomTwoCaptchaPort { get; set; } = 80;
        public string DeathByCaptchaUsername { get; set; } = "";
        public string DeathByCaptchaPassword { get; set; } = "";
        public string DeCaptcherUsername { get; set; } = "";
        public string DeCaptcherPassword { get; set; } = "";
        public string ImageTyperzApiKey { get; set; } = "";
        public string RuCaptchaApiKey { get; set; } = "";
        public string SolveCaptchaApiKey { get; set; } = "";
        public string SolveRecaptchaApiKey { get; set; } = "";
        public string TrueCaptchaUsername { get; set; } = "";
        public string TrueCaptchaApiKey { get; set; } = "";
        public string TwoCaptchaApiKey { get; set; } = "";
        public string NineKWApiKey { get; set; } = "";
        public string CustomAntiCaptchaApiKey { get; set; } = "";
        public string CustomAntiCaptchaDomain { get; set; } = "";
        public int CustomAntiCaptchaPort { get; set; } = 80;
        public string AnyCaptchaApiKey { get; set; } = "";
        public string CapSolverApiKey { get; set; } = "";
    }
}
