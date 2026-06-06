using RuriLib.Functions.Captchas;

namespace RuriLib.Models.Settings;

/// <summary>
/// Stores captcha service configuration and credentials.
/// </summary>
public class CaptchaSettings
{
    /// <summary>
    /// Gets or sets the currently selected captcha service.
    /// </summary>
    public CaptchaServiceType CurrentService { get; set; } = CaptchaServiceType.TwoCaptcha;

    /// <summary>
    /// Gets or sets the solver timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the polling interval in milliseconds.
    /// </summary>
    public int PollingIntervalMilliseconds { get; set; } = 5000;

    /// <summary>
    /// Gets or sets a value indicating whether balance should be checked before solving.
    /// </summary>
    public bool CheckBalanceBeforeSolving { get; set; } = true;

    /// <summary>Gets or sets the Anti-Captcha API key.</summary>
    public string AntiCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the AZCaptcha API key.</summary>
    public string AZCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the CapMonster host.</summary>
    public string CapMonsterHost { get; set; } = "127.0.0.3";
    /// <summary>Gets or sets the CapMonster port.</summary>
    public int CapMonsterPort { get; set; } = 80;
    /// <summary>Gets or sets the Captchas.io API key.</summary>
    public string CaptchasDotIoApiKey { get; set; } = "";
    /// <summary>Gets or sets the custom 2Captcha-compatible API key.</summary>
    public string CustomTwoCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the custom 2Captcha-compatible domain.</summary>
    public string CustomTwoCaptchaDomain { get; set; } = "";
    /// <summary>Gets or sets a value indicating whether the custom 2Captcha host header should be overridden.</summary>
    public bool CustomTwoCaptchaOverrideHostHeader { get; set; } = true;
    /// <summary>Gets or sets the custom 2Captcha-compatible port.</summary>
    public int CustomTwoCaptchaPort { get; set; } = 80;
    /// <summary>Gets or sets the DeathByCaptcha username.</summary>
    public string DeathByCaptchaUsername { get; set; } = "";
    /// <summary>Gets or sets the DeathByCaptcha password.</summary>
    public string DeathByCaptchaPassword { get; set; } = "";
    /// <summary>Gets or sets the CaptchaCoder API key.</summary>
    public string CaptchaCoderApiKey { get; set; } = "";
    /// <summary>Gets or sets the HumanCoder API key.</summary>
    public string HumanCoderApiKey { get; set; } = "";
    /// <summary>Gets or sets the ImageTyperz API key.</summary>
    public string ImageTyperzApiKey { get; set; } = "";
    /// <summary>Gets or sets the RuCaptcha API key.</summary>
    public string RuCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the SolveCaptcha API key.</summary>
    public string SolveCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the TrueCaptcha username.</summary>
    public string TrueCaptchaUsername { get; set; } = "";
    /// <summary>Gets or sets the TrueCaptcha API key.</summary>
    public string TrueCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the 2Captcha API key.</summary>
    public string TwoCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the 9kw API key.</summary>
    public string NineKWApiKey { get; set; } = "";
    /// <summary>Gets or sets the custom Anti-Captcha-compatible API key.</summary>
    public string CustomAntiCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the custom Anti-Captcha-compatible domain.</summary>
    public string CustomAntiCaptchaDomain { get; set; } = "";
    /// <summary>Gets or sets the custom Anti-Captcha-compatible port.</summary>
    public int CustomAntiCaptchaPort { get; set; } = 80;
    /// <summary>Gets or sets the CapMonster Cloud API key.</summary>
    public string CapMonsterCloudApiKey { get; set; } = "";
    /// <summary>Gets or sets the MetaBypassTech client identifier.</summary>
    public string MetaBypassTechClientId { get; set; } = "";
    /// <summary>Gets or sets the MetaBypassTech client secret.</summary>
    public string MetaBypassTechClientSecret { get; set; } = "";
    /// <summary>Gets or sets the MetaBypassTech username.</summary>
    public string MetaBypassTechUsername { get; set; } = "";
    /// <summary>Gets or sets the MetaBypassTech password.</summary>
    public string MetaBypassTechPassword { get; set; } = "";
    /// <summary>Gets or sets the NextCaptcha API key.</summary>
    public string NextCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the NoCaptchaAI API key.</summary>
    public string NoCaptchaAiApiKey { get; set; } = "";
    /// <summary>Gets or sets the NopeCHA API key.</summary>
    public string NopechaApiKey { get; set; } = "";
    /// <summary>Gets or sets the BestCaptchaSolver API key.</summary>
    public string BestCaptchaSolverApiKey { get; set; } = "";
    /// <summary>Gets or sets the CaptchaAI API key.</summary>
    public string CaptchaAiApiKey { get; set; } = "";
    /// <summary>Gets or sets the EzCaptcha API key.</summary>
    public string EzCaptchaApiKey { get; set; } = "";
    /// <summary>Gets or sets the EndCaptcha username.</summary>
    public string EndCaptchaUsername { get; set; } = "";
    /// <summary>Gets or sets the EndCaptcha password.</summary>
    public string EndCaptchaPassword { get; set; } = "";
    /// <summary>Gets or sets the CapGuru API key.</summary>
    public string CapGuruApiKey { get; set; } = "";
    /// <summary>Gets or sets the AYCD API key.</summary>
    public string AycdApiKey { get; set; } = "";
}
