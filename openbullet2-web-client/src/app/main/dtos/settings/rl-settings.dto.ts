export enum ParallelizerType {
  TaskBased = 'taskBased',
  ThreadBased = 'threadBased',
  ParallelBased = 'parallelBased',
}

export enum CaptchaServiceType {
  TwoCaptcha = 'twoCaptcha',
  AntiCaptcha = 'antiCaptcha',
  CustomTwoCaptcha = 'customTwoCaptcha',
  DeathByCaptcha = 'deathByCaptcha',
  CaptchaCoder = 'captchaCoder',
  ImageTyperz = 'imageTyperz',
  CapMonster = 'capMonster',
  AzCaptcha = 'azCaptcha',
  CaptchasIo = 'captchasIo',
  RuCaptcha = 'ruCaptcha',
  SolveCaptcha = 'solveCaptcha',
  TrueCaptcha = 'trueCaptcha',
  NineKw = 'nineKw',
  CustomAntiCaptcha = 'customAntiCaptcha',
  CapSolver = 'capSolver',
  CapMonsterCloud = 'capMonsterCloud',
  HumanCoder = 'humanCoder',
  Nopecha = 'nopecha',
  NoCaptchaAi = 'noCaptchaAi',
  MetaBypassTech = 'metaBypassTech',
  CaptchaAi = 'captchaAi',
  NextCaptcha = 'nextCaptcha',
  EzCaptcha = 'ezCaptcha',
  EndCaptcha = 'endCaptcha',
  BestCaptchaSolver = 'bestCaptchaSolver',
  CapGuru = 'capGuru',
  Aycd = 'aycd',
}

export enum BrowserType {
  Chrome = 'chrome',
  Firefox = 'firefox',
}

export interface RLSettingsDto {
  generalSettings: GeneralRLSettings;
  captchaSettings: CaptchaRLSettings;
  proxySettings: ProxyRLSettings;
  puppeteerSettings: PuppeteerRLSettings;
  seleniumSettings: SeleniumRLSettings;
}

export interface GeneralRLSettings {
  parallelizerType: ParallelizerType;
  logJobActivityToFile: boolean;
  restrictBlocksToCWD: boolean;
  useCustomUserAgentsList: boolean;
  enableBotLogging: boolean;
  verboseMode: boolean;
  logAllResults: boolean;
  userAgents: string[];
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
  captchaCoderApiKey: string;
  humanCoderApiKey: string;
  imageTyperzApiKey: string;
  ruCaptchaApiKey: string;
  solveCaptchaApiKey: string;
  trueCaptchaUsername: string;
  trueCaptchaApiKey: string;
  twoCaptchaApiKey: string;
  nineKWApiKey: string;
  customAntiCaptchaApiKey: string;
  customAntiCaptchaDomain: string;
  customAntiCaptchaPort: number;
  capMonsterCloudApiKey: string;
  metaBypassTechClientId: string;
  metaBypassTechClientSecret: string;
  metaBypassTechUsername: string;
  metaBypassTechPassword: string;
  nextCaptchaApiKey: string;
  noCaptchaAiApiKey: string;
  nopechaApiKey: string;
  bestCaptchaSolverApiKey: string;
  captchaAiApiKey: string;
  ezCaptchaApiKey: string;
  endCaptchaUsername: string;
  endCaptchaPassword: string;
  capGuruApiKey: string;
  aycdApiKey: string;
}

export interface ProxyRLSettings {
  proxyConnectTimeoutMilliseconds: number;
  proxyReadWriteTimeoutMilliseconds: number;
  globalBanKeys: string[];
  globalRetryKeys: string[];
}

export interface PuppeteerRLSettings {
  chromeBinaryLocation: string;
}

export interface SeleniumRLSettings {
  browserType: BrowserType;
  chromeBinaryLocation: string;
  firefoxBinaryLocation: string;
}
