import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CreateTriggeredActionDto, TriggeredActionDto, UpdateTriggeredActionDto } from "../dtos/monitor/triggered-action.dto";
import { getBaseUrl } from "src/app/shared/utils/host";

@Injectable({
    providedIn: 'root'
})
export class JobMonitorService {
    constructor(
        private http: HttpClient
    ) { }

    getAllTriggeredActions() {
        return this.http.get<TriggeredActionDto[]>(
            getBaseUrl() + '/job-monitor/triggered-action/all'
        );
    }

    getTriggeredAction(id: string) {
        return this.http.get<TriggeredActionDto>(
            getBaseUrl() + '/job-monitor/triggered-action',
            {
                params: {
                    id
                }
            }
        );
    }

    createTriggeredAction(dto: CreateTriggeredActionDto) {
        return this.http.post<TriggeredActionDto>(
            getBaseUrl() + '/job-monitor/triggered-action',
            dto
        );
    }

    updateTriggeredAction(dto: UpdateTriggeredActionDto) {
        return this.http.put<TriggeredActionDto>(
            getBaseUrl() + '/job-monitor/triggered-action',
            dto
        );
    }

    deleteTriggeredAction(id: string) {
        return this.http.delete<void>(
            getBaseUrl() + '/job-monitor/triggered-action',
            {
                params: {
                    id
                }
            }
        );
    }

    resetTriggeredAction(id: string) {
        return this.http.post<void>(
            getBaseUrl() + '/job-monitor/triggered-action/reset',
            null,
            {
                params: {
                    id
                }
            }
        );
    }
}