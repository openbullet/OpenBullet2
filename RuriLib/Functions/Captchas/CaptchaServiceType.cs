namespace RuriLib.Functions.Captchas
{
    /// <summary>The available captcha solving service providers.</summary>
    public enum CaptchaServiceType
    {
        /// <summary>The service provided by 2captcha.com</summary>
        TwoCaptcha,

        /// <summary>The service provided by anti-captcha.com</summary>
        AntiCaptcha,

        /// <summary>A service that implements the 2captcha API.</summary>
        CustomTwoCaptcha,

        /// <summary>The service provided by deathbycaptcha.com</summary>
        DeathByCaptcha,

        /// <summary>The service provided by de-captcher.com</summary>
        DeCaptcher,

        /// <summary>The service provided by imagetyperz.com</summary>
        ImageTyperz,

        /// <summary>The service provided by the CapMonster OCR application by ZennoLab.</summary>
        CapMonster,

        /// <summary>The service provided by azcaptcha.com</summary>
        AzCaptcha,

        /// <summary>The service provided by captchas.io</summary>
        CaptchasIO,

        /// <summary>The service provided by rucaptcha.com</summary>
        RuCaptcha,

        /// <summary>The service provided by solvecaptcha.com</summary>
        SolveCaptcha,

        /// <summary>The service provided by solverecaptcha.com</summary>
        SolveRecaptcha,

        /// <summary>The service provided by apitruecaptcha.org</summary>
        TrueCaptcha,

        /// <summary>The service provided by 9kw.eu</summary>
        NineKW,

        /// <summary>A service that implements the anti-captcha API.</summary>
        CustomAntiCaptcha,

        /// <summary>The service provided by anycaptcha.com</summary>
        AnyCaptcha,

        /// <summary>The service provided by capsolver.com</summary>
        CapSolver
    }
}
