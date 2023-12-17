export enum ActionType {
    Wait = 'waitAction',
    SetRelativeStartCondition = 'setRelativeStartConditionAction',
    StopJob = 'stopJobAction',
    AbortJob = 'abortJobAction',
    StartJob = 'startJobAction',
    DiscordWebhook = 'discordWebhookAction',
    TelegramBot = 'telegramBotAction',
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

export type ActionDto =
    WaitActionDto |
    SetRelativeStartConditionActionDto |
    StopJobActionDto |
    AbortJobActionDto |
    StartJobActionDto |
    DiscordWebhookActionDto |
    TelegramBotActionDto;
