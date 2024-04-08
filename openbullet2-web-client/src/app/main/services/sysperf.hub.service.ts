import { EventEmitter, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { getBaseHubUrl } from 'src/app/shared/utils/host';
import { PerformanceInfoDto } from '../dtos/info/performance-info.dto';
import { UserService } from './user.service';

@Injectable({ providedIn: 'root' })
export class SysPerfHubService {
  private hubConnection: HubConnection | null = null;

  private metricsEmitter = new EventEmitter<PerformanceInfoDto | null>();
  public metrics$ = this.metricsEmitter.asObservable();

  constructor(private userService: UserService) {}

  createHubConnection() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${getBaseHubUrl()}/system-performance`, {
        accessTokenFactory: () => this.userService.getJwt() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch((err) => console.error(err));

    this.hubConnection.on('newMetrics', (metrics) => {
      this.metricsEmitter.emit(metrics);
    });
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch((err) => console.error(err));
  }
}
