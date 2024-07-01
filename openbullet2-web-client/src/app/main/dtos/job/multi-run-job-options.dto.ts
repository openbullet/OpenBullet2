import { ProxyType } from '../../enums/proxy-type';
import { AbsoluteTimeStartConditionDto, RelativeTimeStartConditionDto } from './start-condition.dto';

export enum JobProxyMode {
  Default = 'default',
  On = 'on',
  Off = 'off',
}

export enum NoValidProxyBehaviour {
  DoNothing = 'doNothing',
  Unban = 'unban',
  Reload = 'reload',
}

export interface MultiRunJobOptionsDto {
  name: string;
  startCondition: RelativeTimeStartConditionDto | AbsoluteTimeStartConditionDto;
  configId: string | null;
  bots: number;
  skip: number;
  proxyMode: JobProxyMode;
  shuffleProxies: boolean;
  noValidProxyBehaviour: NoValidProxyBehaviour;
  proxyBanTimeSeconds: number;
  markAsToCheckOnAbort: boolean;
  neverBanProxies: boolean;
  concurrentProxyMode: boolean;
  periodicReloadIntervalSeconds: number;
  dataPool: DataPoolTypes;
  proxySources: ProxySourceTypes[];
  hitOutputs: HitOutputTypes[];
}

export type DataPoolTypes = WordlistDataPool | FileDataPool | RangeDataPool | CombinationsDataPool | InfiniteDataPool;
export type ProxySourceTypes = GroupProxySource | FileProxySource | RemoteProxySource;
export type HitOutputTypes =
  | DatabaseHitOutput
  | FileSystemHitOutput
  | DiscordWebhookHitOutput
  | TelegramBotHitOutput
  | CustomWebhookHitOutput;

export enum DataPoolType {
  Wordlist = 'wordlistDataPool',
  File = 'fileDataPool',
  Range = 'rangeDataPool',
  Combinations = 'combinationsDataPool',
  Infinite = 'infiniteDataPool',
}

export interface WordlistDataPool {
  _polyTypeName: DataPoolType.Wordlist;
  wordlistId: number;
}

export interface FileDataPool {
  _polyTypeName: DataPoolType.File;
  wordlistType: string;
  fileName: string;
}

export interface RangeDataPool {
  _polyTypeName: DataPoolType.Range;
  wordlistType: string;
  start: number;
  amount: number;
  step: number;
  pad: boolean;
}

export interface CombinationsDataPool {
  _polyTypeName: DataPoolType.Combinations;
  wordlistType: string;
  charSet: string;
  length: number;
}

export interface InfiniteDataPool {
  _polyTypeName: DataPoolType.Infinite;
}

export enum ProxySourceType {
  Group = 'groupProxySource',
  File = 'fileProxySource',
  Remote = 'remoteProxySource',
}

export interface GroupProxySource {
  _polyTypeName: ProxySourceType.Group;
  groupId: number;
}

export interface FileProxySource {
  _polyTypeName: ProxySourceType.File;
  fileName: string;
  defaultType: ProxyType;
}

export interface RemoteProxySource {
  _polyTypeName: ProxySourceType.Remote;
  url: string;
  defaultType: ProxyType;
}

export enum HitOutputType {
  Database = 'databaseHitOutput',
  FileSystem = 'fileSystemHitOutput',
  DiscordWebhook = 'discordWebhookHitOutput',
  TelegramBot = 'telegramBotHitOutput',
  CustomWebhook = 'customWebhookHitOutput',
}

export interface DatabaseHitOutput {
  _polyTypeName: HitOutputType.Database;
}

export interface FileSystemHitOutput {
  _polyTypeName: HitOutputType.FileSystem;
  baseDir: string;
}

export interface DiscordWebhookHitOutput {
  _polyTypeName: HitOutputType.DiscordWebhook;
  webhook: string;
  username: string;
  avatarUrl: string;
  onlyHits: boolean;
}

export interface TelegramBotHitOutput {
  _polyTypeName: HitOutputType.TelegramBot;
  apiServer: string;
  token: string;
  chatId: number;
  onlyHits: boolean;
}

export interface CustomWebhookHitOutput {
  _polyTypeName: HitOutputType.CustomWebhook;
  url: string;
  user: string;
  onlyHits: boolean;
}
