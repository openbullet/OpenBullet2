namespace RuriLib.Functions.Captchas;

/// <summary>
/// The available captcha solving service providers.
/// </summary>
public enum CaptchaServiceType
{
    // IMPORTANT: The enum values should not be changed because they are
    // used to identify the service in the settings file.
        
    /// <summary>The service provided by https://2captcha.com/</summary>
    TwoCaptcha = 0,

    /// <summary>The service provided by https://anti-captcha.com/</summary>
    AntiCaptcha = 1,

    /// <summary>A service that implements the 2captcha API.</summary>
    CustomTwoCaptcha = 2,

    /// <summary>The service provided by https://deathbycaptcha.com/</summary>
    DeathByCaptcha = 3,

    /// <summary>The service provided by https://captchacoder.com/</summary>
    CaptchaCoder = 4,

    /// <summary>The service provided by https://www.imagetyperz.com/</summary>
    ImageTyperz = 5,

    /// <summary>The service provided by the CapMonster OCR application by ZennoLab.</summary>
    CapMonster = 6,

    /// <summary>The service provided by https://azcaptcha.com/</summary>
    AzCaptcha = 7,

    /// <summary>The service provided by https://captchas.io/</summary>
    CaptchasIo = 8,

    /// <summary>The service provided by https://rucaptcha.com/</summary>
    RuCaptcha = 9,

    /// <summary>The service provided by https://solvecaptcha.net/</summary>
    SolveCaptcha = 10,

    /// <summary>The service provided by https://truecaptcha.com/</summary>
    TrueCaptcha = 12,

    /// <summary>The service provided by https://www.9kw.eu/</summary>
    NineKw = 13,

    /// <summary>A service that implements the anti-captcha API.</summary>
    CustomAntiCaptcha = 14,

    /// <summary>The service provided by https://capsolver.com/</summary>
    CapSolver = 16,
        
    /// <summary>The service provided by https://capmonster.cloud/</summary>
    CapMonsterCloud = 17,
        
    /// <summary>The service provided by https://humancoder.com/</summary>
    HumanCoder = 18,
        
    /// <summary>The service provided by https://nopecha.com/</summary>
    Nopecha = 19,
        
    /// <summary>The service provided by https://nocaptchaai.com/</summary>
    NoCaptchaAi = 20,
        
    /// <summary>The service provided by https://metabypass.tech/</summary>
    MetaBypassTech = 21,
        
    /// <summary>The service provided by https://captchai.com/</summary>
    CaptchaAi = 22,
        
    /// <summary>The service provided by https://nextcaptcha.com/</summary>
    NextCaptcha = 23,
        
    /// <summary>The service provided by https://ez-captcha.com/</summary>
    EzCaptcha = 24,
        
    /// <summary>The service provided by https://endcaptcha.com/</summary>
    EndCaptcha = 25,
        
    /// <summary>The service provided by https://bestcaptchasolver.com/</summary>
    BestCaptchaSolver = 26,
        
    /// <summary>The service provided by https://cap.guru/</summary>
    CapGuru = 27,
    
    /// <summary>The service provided by https://aycd.io/</summary>
    Aycd = 28,
}
