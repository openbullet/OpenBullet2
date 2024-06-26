import { EventEmitter, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { getBaseHubUrl } from 'src/app/shared/utils/host';
import { ErrorMessage } from '../dtos/common/messages.dto';
import {
  DbgNewLogMessage,
  DbgStateDto,
  DbgStatusChangedMessage,
  DbgVariablesChangedMessage,
} from '../dtos/config-debugger/messages';
import { ConfigDebuggerSettings } from '../models/config-debugger-settings';
import { UserService } from './user.service';

@Injectable({ providedIn: 'root' })
export class ConfigDebuggerHubService {
  private hubConnection: HubConnection | null = null;

  private logsEmitter = new EventEmitter<DbgNewLogMessage | null>();
  public logs$ = this.logsEmitter.asObservable();

  private statusEmitter = new EventEmitter<DbgStatusChangedMessage | null>();
  public status$ = this.statusEmitter.asObservable();

  private variablesEmitter = new EventEmitter<DbgVariablesChangedMessage | null>();
  public variables$ = this.variablesEmitter.asObservable();

  private stateEmitter = new EventEmitter<DbgStateDto | null>();
  public state$ = this.stateEmitter.asObservable();

  private errorEmitter = new EventEmitter<ErrorMessage | null>();
  public error$ = this.errorEmitter.asObservable();

  constructor(private userService: UserService) { }

  createHubConnection(configId: string) {
    const encodedConfigId = encodeURIComponent(configId);
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${getBaseHubUrl()}/config-debugger?configId=${encodedConfigId}`, {
        accessTokenFactory: () => this.userService.getJwt() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('newLogEntry', (msg) => {
      this.logsEmitter.emit(msg);
    });

    this.hubConnection.on('statusChanged', (msg) => {
      this.statusEmitter.emit(msg);
    });

    this.hubConnection.on('variablesChanged', (msg) => {
      this.variablesEmitter.emit(msg);
    });

    this.hubConnection.on('debuggerState', (msg) => {
      this.stateEmitter.emit(msg);
    });

    this.hubConnection.on('error', (msg) => {
      this.errorEmitter.emit(msg);
    });

    return this.hubConnection.start().catch((err) => console.error(err));
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch((err) => console.error(err));
  }

  async start(settings: ConfigDebuggerSettings) {
    return this.hubConnection
      ?.invoke('start', {
        testData: settings.testData,
        wordlistType: settings.wordlistType,
        useProxy: settings.useProxy,
        testProxy: settings.testProxy,
        proxyType: settings.proxyType,
        persistLog: settings.persistLog,
        stepByStep: settings.stepByStep,
      })
      .catch((error) => console.log(error));
  }

  async stop() {
    return this.hubConnection?.invoke('stop').catch((error) => console.log(error));
  }

  async takeStep() {
    return this.hubConnection?.invoke('takeStep').catch((error) => console.log(error));
  }

  async getState() {
    return this.hubConnection?.invoke('getState').catch((error) => console.log(error));
  }
}
