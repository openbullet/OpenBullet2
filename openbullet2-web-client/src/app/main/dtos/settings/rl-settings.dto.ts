export enum ParallelizerType {
    TaskBased = 'taskBased',
    ThreadBased = 'threadBased',
    ParallelBased = 'parallelBased'
}

export enum CaptchaServiceType {
    TwoCaptcha = 'twoCaptcha',
    AntiCaptcha = 'antiCaptcha',
    CustomTwoCaptcha = 'customTwoCaptcha',
    DeathByCaptcha = 'deathByCaptcha',
    DeCaptcher = 'deCaptcher',
    ImageTyperz = 'imageTyperz',
    CapMonster = 'capMonster',
    AzCaptcha = 'azCaptcha',
    CaptchasIO = 'captchasIO',
    RuCaptcha = 'ruCaptcha',
    SolveCaptcha = 'solveCaptcha',
    SolveRecaptcha = 'solveRecaptcha',
    TrueCaptcha = 'trueCaptcha',
    NineKW = 'nineKW',
    CustomAntiCaptcha = 'customAntiCaptcha',
    AnyCaptcha = 'anyCaptcha',
    CapSolver = 'capSolver'
}

export enum BrowserType {
    Chrome = 'chrome',
    Firefox = 'firefox'
}

export interface RLSettingsDto {
    generalSettings: GeneralRLSettings;
    captchaSettings: CaptchaRLSettings;
    proxySettings: ProxyRLSettings;
    puppeteerSettings: PuppeteerRLSettings;
    seleniumSettings: SeleniumRLSettings
}

export interface GeneralRLSettings {
    parallelizerType: ParallelizerType;
    logJobActivityToFile: boolean;
    restrictBlocksToCWD: boolean;
    useCustomUserAgentsList: boolean;
    enableBotLogging: boolean;
    verboseMode: boolean;
    logAllResults: boolean;
    userAgents: string[]
}

export interface CaptchaRLSettings {
    currentService: CaptchaServiceType;
    timeoutSeconds: number;
    pollingIntervalMilliseconds: number;
    checkBalanceBeforeSolving: boolean;
    antiCaptchaApiKey: string;
    azCaptchaApiKey: string;
    capMonsterHost: string;
    capMonsterPort: number;
    captchasDotIoApiKey: string;
    customTwoCaptchaApiKey: string;
    customTwoCaptchaDomain: string;
    customTwoCaptchaOverrideHostHeader: boolean;
    customTwoCaptchaPort: number;
    deathByCaptchaUsername: string;
    deathByCaptchaPassword: string;
    deCaptcherUsername: string;
    deCaptcherPassword: string;
    imageTyperzApiKey: string;
    ruCaptchaApiKey: string;
    solveCaptchaApiKey: string;
    solveRecaptchaApiKey: string;
    trueCaptchaUsername: string;
    trueCaptchaApiKey: string;
    twoCaptchaApiKey: string;
    nineKWApiKey: string;
    customAntiCaptchaApiKey: string;
    customAntiCaptchaDomain: string;
    customAntiCaptchaPort: number;
    anyCaptchaApiKey: string;
    capSolverApiKey: string;
}

export interface ProxyRLSettings {
    proxyConnectTimeoutMilliseconds: number;
    proxyReadWriteTimeoutMilliseconds: number;
    globalBanKeys: string[];
    globalRetryKeys: string[]
}

export interface PuppeteerRLSettings {
    chromeBinaryLocation: string
}

export interface SeleniumRLSettings {
    browserType: BrowserType;
    chromeBinaryLocation: string;
    firefoxBinaryLocation: string
}
