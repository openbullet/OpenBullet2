export interface OBSettingsDto {
    generalSettings: GeneralOBSettings;
    remoteSettings: RemoteOBSettings;
    securitySettings: SecurityOBSettings;
    customizationSettings: CustomizationOBSettings
}

export interface GeneralOBSettings {
    configSectionOnLoad: string;
    autoSetRecommendedBots: boolean;
    warnConfigNotSaved: boolean;
    defaultAuthor: string;
    enableJobLogging: boolean;
    logBufferSize: number;
    ignoreWordlistNameOnHitsDedupe: boolean;
    proxyCheckTargets: ProxyCheckTarget[];
    defaultJobDisplayMode: string;
    jobUpdateInterval: number;
    jobManagerUpdateInterval: number;
    groupCapturesInDebugger: boolean;
    culture: string;
    customSnippets: CustomSnippet[]
}

export interface ProxyCheckTarget {
    url: string;
    successKey: string
}

export interface CustomSnippet {
    name: string;
    body: string;
    description: string
}

export interface RemoteOBSettings {
    configsEndpoints: RemoteConfigsEndpoint[]
}

export interface RemoteConfigsEndpoint {
    url: string;
    apiKey: string
}

export interface SecurityOBSettings {
    allowSystemWideFileAccess: boolean;
    requireAdminLogin: boolean;
    adminUsername: string;
    adminSessionLifetimeHours: number;
    guestSessionLifetimeHours: number;
    httpsRedirect: boolean
}

export interface CustomizationOBSettings {
    theme: string;
    monacoTheme: string;
    wordWrap: boolean;
    playSoundOnHit: boolean
}

export interface SafeOBSettingsDto {
    generalSettings: SafeGeneralOBSettings;
}

export interface SafeGeneralOBSettings {
    jobManagerUpdateInterval: number;
}
