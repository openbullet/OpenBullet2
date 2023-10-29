import { Injectable } from "@angular/core";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { BehaviorSubject } from "rxjs";
import { getBaseHubUrl } from "src/app/shared/utils/host";
import { UserService } from "./user.service";
import { PCJNewResultMessage } from "../dtos/job/messages/proxy-check/new-result.dto";

@Injectable({providedIn: 'root'})
export class ProxyCheckJobHubService {
    private hubConnection: HubConnection | null = null;
    private resultSource = new BehaviorSubject<PCJNewResultMessage | null>(null);
    public result$ = this.resultSource.asObservable();

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
            this.resultSource.next(result)
        });
    }

    stopHubConnection() {
        this.hubConnection
        ?.stop()
        .catch(err => console.error(err));
    }
}