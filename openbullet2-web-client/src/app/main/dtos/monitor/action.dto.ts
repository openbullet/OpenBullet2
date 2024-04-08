import { TimeSpanPipe } from 'src/app/shared/pipes/timespan.pipe';
import { parseTimeSpan } from 'src/app/shared/utils/dates';

export enum ActionType {
  Wait = 'waitAction',
  SetRelativeStartCondition = 'setRelativeStartConditionAction',
  StopJob = 'stopJobAction',
  AbortJob = 'abortJobAction',
  StartJob = 'startJobAction',
  DiscordWebhook = 'discordWebhookAction',
  TelegramBot = 'telegramBotAction',
  SetBots = 'setBotsAction',
  ReloadProxies = 'reloadProxiesAction',
}

export interface WaitActionDto {
  _polyTypeName: ActionType.Wait;
  timeSpan: string;
}

export interface SetRelativeStartConditionActionDto {
  _polyTypeName: ActionType.SetRelativeStartCondition;
  jobId: number;
  timeSpan: string;
}

export interface StopJobActionDto {
  _polyTypeName: ActionType.StopJob;
  jobId: number;
}

export interface AbortJobActionDto {
  _polyTypeName: ActionType.AbortJob;
  jobId: number;
}

export interface StartJobActionDto {
  _polyTypeName: ActionType.StartJob;
  jobId: number;
}

export interface DiscordWebhookActionDto {
  _polyTypeName: ActionType.DiscordWebhook;
  webhook: string;
  message: string;
}

export interface TelegramBotActionDto {
  _polyTypeName: ActionType.TelegramBot;
  apiServer: string;
  token: string;
  chatId: number;
  message: string;
}

export interface SetBotsActionDto {
  _polyTypeName: ActionType.SetBots;
  amount: number;
}

export interface ReloadProxiesActionDto {
  _polyTypeName: ActionType.ReloadProxies;
}

export type ActionDto =
  | WaitActionDto
  | SetRelativeStartConditionActionDto
  | StopJobActionDto
  | AbortJobActionDto
  | StartJobActionDto
  | DiscordWebhookActionDto
  | TelegramBotActionDto
  | SetBotsActionDto
  | ReloadProxiesActionDto;

export function getActionText(action: ActionDto): string {
  const timeSpanPipe = new TimeSpanPipe();

  switch (action._polyTypeName) {
    case ActionType.Wait:
      return `Wait ${timeSpanPipe.transform(parseTimeSpan(action.timeSpan))}`;
    case ActionType.SetRelativeStartCondition:
      return `Set relative start condition of job ${action.jobId} to ${timeSpanPipe.transform(
        parseTimeSpan(action.timeSpan),
      )}`;
    case ActionType.StopJob:
      return `Stop job ${action.jobId}`;
    case ActionType.AbortJob:
      return `Abort job ${action.jobId}`;
    case ActionType.StartJob:
      return `Start job ${action.jobId}`;
    case ActionType.DiscordWebhook:
      return 'Send message via Discord webhook';
    case ActionType.TelegramBot:
      return 'Send message via Telegram bot';
    case ActionType.SetBots:
      return `Set bots to ${action.amount}`;
    case ActionType.ReloadProxies:
      return 'Reload proxies';
    default:
      return 'Unknown action';
  }
}
