import { ProxyType } from '../../enums/proxy-type';
import { ConfigMode } from './config-info.dto';

export enum StringRule {
  EqualTo = 'equalTo',
  Contains = 'contains',
  LongerThan = 'longerThan',
  ShorterThan = 'shorterThan',
  ContainsAll = 'containsAll',
  ContainsAny = 'containsAny',
  StartsWith = 'startsWith',
  EndsWith = 'endsWith',
}

export interface ConfigDto {
  id: string;
  isRemote: boolean;
  mode: ConfigMode;
  metadata: ConfigMetadataDto;
  settings: ConfigSettingsDto;
  readme: string;
  loliCodeScript: string;
  startupLoliCodeScript: string;
  loliScript: string;
  cSharpScript: string;
  startupCSharpScript: string;
}

export interface ConfigMetadataDto {
  name: string;
  category: string;
  author: string;
  base64Image: string;
  creationDate: string;
  lastModified: string;
  plugins: string[];
}

export interface ConfigSettingsDto {
  generalSettings: ConfigGeneralSettingsDto;
  proxySettings: ConfigProxySettingsDto;
  inputSettings: ConfigInputSettingsDto;
  dataSettings: ConfigDataSettingsDto;
  browserSettings: ConfigBrowserSettingsDto;
  scriptSettings: ConfigScriptSettingsDto;
}

export interface ConfigGeneralSettingsDto {
  suggestedBots: number;
  maximumCPM: number;
  saveEmptyCaptures: boolean;
  reportLastCaptchaOnRetry: boolean;
  continueStatuses: string[];
}

export interface ConfigProxySettingsDto {
  useProxies: boolean;
  maxUsesPerProxy: number;
  banLoopEvasion: number;
  banProxyStatuses: string[];
  allowedProxyTypes: ProxyType[];
}

export interface ConfigInputSettingsDto {
  customInputs: CustomInputDto[];
}

export interface CustomInputDto {
  description: string;
  variableName: string;
  defaultAnswer: string;
}

export interface ConfigDataSettingsDto {
  allowedWordlistTypes: string[];
  urlEncodeDataAfterSlicing: boolean;
  dataRules: DataRulesDto;
  resources: ResourcesDto;
}

export interface DataRulesDto {
  simple: SimpleDataRuleDto[];
  regex: RegexDataRuleDto[];
}

export interface SimpleDataRuleDto {
  sliceName: string;
  invert: boolean;
  comparison: StringRule;
  stringToCompare: string;
  caseSensitive: boolean;
}

export interface RegexDataRuleDto {
  sliceName: string;
  regexToMatch: string;
  invert: boolean;
}

export interface ResourcesDto {
  linesFromFile: LinesFromFileResourceDto[];
  randomLinesFromFile: RandomLinesFromFileResourceDto[];
}

export interface LinesFromFileResourceDto {
  name: string;
  location: string;
  loopsAround: boolean;
  ignoreEmptyLines: boolean;
}

export interface RandomLinesFromFileResourceDto {
  name: string;
  location: string;
  ignoreEmptyLines: boolean;
  unique: boolean;
}

export interface ConfigBrowserSettingsDto {
  quitBrowserStatuses: string[];
  headless: boolean;
  commandLineArgs: string;
  ignoreHttpsErrors: boolean;
  loadOnlyDocumentAndScript: boolean;
  dismissDialogs: boolean;
  blockedUrls: string[];
}

export interface ConfigScriptSettingsDto {
  customUsings: string[];
}
