import { Injectable } from "@angular/core";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { BehaviorSubject } from "rxjs";
import { getBaseHubUrl } from "src/app/shared/utils/host";
import { UserService } from "./user.service";
import { PCJNewResultMessage } from "../dtos/job/messages/proxy-check/new-result.dto";
import { PCJStatsMessage } from "../dtos/job/messages/proxy-check/stats.dto";
import { JobStatusChangedMessage } from "../dtos/job/messages/status-changed.dto";

@Injectable({providedIn: 'root'})
export class ProxyCheckJobHubService {
    private hubConnection: HubConnection | null = null;

    private resultSource = new BehaviorSubject<PCJNewResultMessage | null>(null);
    public result$ = this.resultSource.asObservable();

    private tickSource = new BehaviorSubject<PCJStatsMessage | null>(null);
    public tick$ = this.tickSource.asObservable();

    private statusSource = new BehaviorSubject<JobStatusChangedMessage | null>(null);
    public status$ = this.statusSource.asObservable();

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
    }

    stopHubConnection() {
        this.hubConnection
        ?.stop()
        .catch(err => console.error(err));
    }
}