import { Injectable } from "@angular/core";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { BehaviorSubject } from "rxjs";
import { getBaseHubUrl } from "src/app/shared/utils/host";
import { DbgNewLogMessage, DbgStateDto, DbgStatusChangedMessage, DbgVariablesChangedMessage } from "../dtos/config-debugger/messages";
import { ErrorMessage } from "../dtos/common/messages.dto";
import { ConfigDebuggerSettings } from "../models/config-debugger-settings";

@Injectable({providedIn: 'root'})
export class ConfigDebuggerHubService {
    private hubConnection: HubConnection | null = null;
    
    private logsSource = new BehaviorSubject<DbgNewLogMessage | null>(null);
    public logs$ = this.logsSource.asObservable();

    private statusSource = new BehaviorSubject<DbgStatusChangedMessage | null>(null);
    public status$ = this.statusSource.asObservable();

    private variablesSource = new BehaviorSubject<DbgVariablesChangedMessage | null>(null);
    public variables$ = this.variablesSource.asObservable();

    private stateSource = new BehaviorSubject<DbgStateDto | null>(null);
    public state$ = this.stateSource.asObservable();

    private errorSource = new BehaviorSubject<ErrorMessage | null>(null);
    public error$ = this.errorSource.asObservable();

    createHubConnection(configId: string) {
        // TODO: Add jwt injector here
        this.hubConnection = new HubConnectionBuilder()
        .withUrl(`${getBaseHubUrl()}/config-debugger?configId=${configId}`)
        .withAutomaticReconnect()
        .build();

        this.hubConnection.on('newLogEntry', msg => {
            this.logsSource.next(msg)
        });

        this.hubConnection.on('statusChanged', msg => {
            this.statusSource.next(msg)
        });

        this.hubConnection.on('variablesChanged', msg => {
            this.variablesSource.next(msg)
        });

        this.hubConnection.on('debuggerState', msg => {
            this.stateSource.next(msg)
        });

        this.hubConnection.on('error', msg => {
            this.errorSource.next(msg)
        });

        return this.hubConnection
        .start()
        .catch(err => console.error(err));
    }

    stopHubConnection() {
        this.hubConnection
        ?.stop()
        .catch(err => console.error(err));
    }

    async start(settings: ConfigDebuggerSettings) {
        return this.hubConnection?.invoke('start', { 
            testData: settings.testData,
            wordlistType: settings.wordlistType,
            useProxy: settings.useProxy,
            testProxy: settings.testProxy,
            proxyType: settings.proxyType,
            persistLog: settings.persistLog,
            stepByStep: settings.stepByStep
        })
        .catch(error => console.log(error));
    }

    async stop() {
        return this.hubConnection?.invoke('stop')
        .catch(error => console.log(error));
    }

    async takeStep() {
        return this.hubConnection?.invoke('takeStep')
        .catch(error => console.log(error));
    }

    async getState() {
        return this.hubConnection?.invoke('getState')
        .catch(error => console.log(error));
    }
}