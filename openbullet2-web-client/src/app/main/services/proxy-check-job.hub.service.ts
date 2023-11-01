import { Injectable } from "@angular/core";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { BehaviorSubject } from "rxjs";
import { getBaseHubUrl } from "src/app/shared/utils/host";
import { UserService } from "./user.service";
import { PCJNewResultMessage } from "../dtos/job/messages/proxy-check/new-result.dto";
import { PCJStatsMessage } from "../dtos/job/messages/proxy-check/stats.dto";
import { JobStatusChangedMessage } from "../dtos/job/messages/status-changed.dto";
import { ChangeBotsMessage } from "../dtos/job/messages/change-bots.dto";
import { BotsChangedMessage } from "../dtos/job/messages/bots-changed.dto";

@Injectable({providedIn: 'root'})
export class ProxyCheckJobHubService {
    private hubConnection: HubConnection | null = null;

    private resultSource = new BehaviorSubject<PCJNewResultMessage | null>(null);
    public result$ = this.resultSource.asObservable();

    private tickSource = new BehaviorSubject<PCJStatsMessage | null>(null);
    public tick$ = this.tickSource.asObservable();

    private statusSource = new BehaviorSubject<JobStatusChangedMessage | null>(null);
    public status$ = this.statusSource.asObservable();

    private botsSource = new BehaviorSubject<BotsChangedMessage | null>(null);
    public bots$ = this.botsSource.asObservable();

    constructor(private userService: UserService) {

    }

    createHubConnection(jobId: number) {
        this.hubConnection = new HubConnectionBuilder()
        .withUrl(getBaseHubUrl() + `/proxy-check-job?jobId=${jobId}`, {
            accessTokenFactory: () => this.userService.getJwt() ?? ''
        })
        .withAutomaticReconnect()
        .build();

        this.hubConnection
        .start()
        .catch(err => console.error(err));

        this.hubConnection.on('newResult', result => {
            this.resultSource.next(result);
        });

        this.hubConnection.on('timerTick', tick => {
            this.tickSource.next(tick);
        });

        this.hubConnection.on('statusChanged', status => {
            this.statusSource.next(status);
        });

        this.hubConnection.on('botsChanged', bots => {
            this.botsSource.next(bots);
        });
    }

    start() {
        this.hubConnection
            ?.invoke('start')
            .catch(err => console.error(err));
    }

    pause() {
        this.hubConnection
            ?.invoke('pause')
            .catch(err => console.error(err));
    }

    resume() {
        this.hubConnection
            ?.invoke('resume')
            .catch(err => console.error(err));
    }

    stop() {
        this.hubConnection
            ?.invoke('stop')
            .catch(err => console.error(err));
    }

    abort() {
        this.hubConnection
            ?.invoke('abort')
            .catch(err => console.error(err));
    }

    skipWait() {
        this.hubConnection
            ?.invoke('skipWait')
            .catch(err => console.error(err));
    }

    changeBots(message: ChangeBotsMessage) {
        this.hubConnection
            ?.invoke('changeBots', message)
            .catch(err => console.error(err));
    }

    stopHubConnection() {
        this.hubConnection
            ?.stop()
            .catch(err => console.error(err));
    }
}