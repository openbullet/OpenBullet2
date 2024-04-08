import { EventEmitter, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { getBaseHubUrl } from 'src/app/shared/utils/host';
import { ErrorMessage } from '../dtos/common/messages.dto';
import { BotsChangedMessage } from '../dtos/job/messages/bots-changed.dto';
import { ChangeBotsMessage } from '../dtos/job/messages/change-bots.dto';
import { PCJNewResultMessage } from '../dtos/job/messages/proxy-check/new-result.dto';
import { PCJStatsMessage } from '../dtos/job/messages/proxy-check/stats.dto';
import { PCJTaskErrorMessage } from '../dtos/job/messages/proxy-check/task-error.dto';
import { JobStatusChangedMessage } from '../dtos/job/messages/status-changed.dto';
import { UserService } from './user.service';

@Injectable({ providedIn: 'root' })
export class ProxyCheckJobHubService {
  private hubConnection: HubConnection | null = null;

  private resultEmitter = new EventEmitter<PCJNewResultMessage | null>();
  public result$ = this.resultEmitter.asObservable();

  private tickEmitter = new EventEmitter<PCJStatsMessage | null>();
  public tick$ = this.tickEmitter.asObservable();

  private statusEmitter = new EventEmitter<JobStatusChangedMessage | null>();
  public status$ = this.statusEmitter.asObservable();

  private botsEmitter = new EventEmitter<BotsChangedMessage | null>();
  public bots$ = this.botsEmitter.asObservable();

  private taskErrorEmitter = new EventEmitter<PCJTaskErrorMessage | null>();
  public taskError$ = this.taskErrorEmitter.asObservable();

  private errorEmitter = new EventEmitter<ErrorMessage | null>();
  public error$ = this.errorEmitter.asObservable();

  private completedEmitter = new EventEmitter<boolean | null>();
  public completed$ = this.completedEmitter.asObservable();

  constructor(private userService: UserService) {}

  createHubConnection(jobId: number) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${getBaseHubUrl()}/proxy-check-job?jobId=${jobId}`, {
        accessTokenFactory: () => this.userService.getJwt() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch((err) => console.error(err));

    this.hubConnection.on('newResult', (result) => {
      this.resultEmitter.emit(result);
    });

    this.hubConnection.on('timerTick', (tick) => {
      this.tickEmitter.emit(tick);
    });

    this.hubConnection.on('statusChanged', (status) => {
      this.statusEmitter.emit(status);
    });

    this.hubConnection.on('botsChanged', (bots) => {
      this.botsEmitter.emit(bots);
    });

    this.hubConnection.on('taskError', (error) => {
      this.taskErrorEmitter.emit(error);
    });

    this.hubConnection.on('error', (error) => {
      this.errorEmitter.emit(error);
    });

    this.hubConnection.on('completed', () => {
      this.completedEmitter.emit(true);
    });
  }

  start() {
    this.hubConnection?.invoke('start').catch((err) => console.error(err));
  }

  pause() {
    this.hubConnection?.invoke('pause').catch((err) => console.error(err));
  }

  resume() {
    this.hubConnection?.invoke('resume').catch((err) => console.error(err));
  }

  stop() {
    this.hubConnection?.invoke('stop').catch((err) => console.error(err));
  }

  abort() {
    this.hubConnection?.invoke('abort').catch((err) => console.error(err));
  }

  skipWait() {
    this.hubConnection?.invoke('skipWait').catch((err) => console.error(err));
  }

  changeBots(message: ChangeBotsMessage) {
    this.hubConnection?.invoke('changeBots', message).catch((err) => console.error(err));
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch((err) => console.error(err));
  }
}
