export enum ConfigSection {
  Metadata = 'metadata',
  Readme = 'readme',
  Stacker = 'stacker',
  LoliCode = 'loliCode',
  Settings = 'settings',
  CSharpCode = 'cSharpCode',
  LoliScript = 'loliScript',
}

export enum JobDisplayMode {
  Standard = 'standard',
  Detailed = 'detailed',
}

export interface OBSettingsDto {
  generalSettings: GeneralOBSettings;
  remoteSettings: RemoteOBSettings;
  securitySettings: SecurityOBSettings;
  customizationSettings: CustomizationOBSettings;
}

export interface GeneralOBSettings {
  configSectionOnLoad: ConfigSection;
  autoSetRecommendedBots: boolean;
  warnConfigNotSaved: boolean;
  defaultAuthor: string;
  enableJobLogging: boolean;
  logBufferSize: number;
  ignoreWordlistNameOnHitsDedupe: boolean;
  proxyCheckTargets: ProxyCheckTarget[];
  defaultJobDisplayMode: JobDisplayMode;
  jobUpdateInterval: number;
  jobManagerUpdateInterval: number;
  groupCapturesInDebugger: boolean;
  culture: string;
  customSnippets: CustomSnippet[];
}

export interface ProxyCheckTarget {
  url: string;
  successKey: string;
}

export interface CustomSnippet {
  name: string;
  body: string;
  description: string;
}

export interface RemoteOBSettings {
  configsEndpoints: RemoteConfigsEndpoint[];
}

export interface RemoteConfigsEndpoint {
  url: string;
  apiKey: string;
}

export interface SecurityOBSettings {
  allowSystemWideFileAccess: boolean;
  requireAdminLogin: boolean;
  adminUsername: string;
  adminApiKey: string;
  adminSessionLifetimeHours: number;
  guestSessionLifetimeHours: number;
  httpsRedirect: boolean;
}

export interface CustomizationOBSettings {
  theme: string;
  monacoTheme: string;
  wordWrap: boolean;
  playSoundOnHit: boolean;
}

export interface SafeOBSettingsDto {
  generalSettings: SafeGeneralOBSettings;
  customizationSettings: SafeCustomizationOBSettings;
}

export interface SafeGeneralOBSettings {
  configSectionOnLoad: ConfigSection;
  jobManagerUpdateInterval: number;
  defaultJobDisplayMode: JobDisplayMode;
}

export interface SafeCustomizationOBSettings {
  playSoundOnHit: boolean;
  wordWrap: boolean;
}
