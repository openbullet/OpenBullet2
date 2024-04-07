import { BotLoggerEntry } from './bot-logger-entry';

export interface DbgNewLogMessage {
  newMessage: BotLoggerEntry;
}

export interface DbgStatusChangedMessage {
  newStatus: string;
}

export interface VariableDto {
  name: string;
  markedForCapture: boolean;
  type: string;
  value: any;
}

export interface DbgVariablesChangedMessage {
  variables: VariableDto[];
}

export interface DbgStateDto {
  status: string;
  log: BotLoggerEntry[];
  variables: VariableDto[];
}
