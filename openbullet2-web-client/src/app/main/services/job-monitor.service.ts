import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getBaseUrl } from 'src/app/shared/utils/host';
import {
  CreateTriggeredActionDto,
  TriggeredActionDto,
  UpdateTriggeredActionDto,
} from '../dtos/monitor/triggered-action.dto';

@Injectable({
  providedIn: 'root',
})
export class JobMonitorService {
  constructor(private http: HttpClient) {}

  getAllTriggeredActions() {
    return this.http.get<TriggeredActionDto[]>(`${getBaseUrl()}/job-monitor/triggered-action/all`);
  }

  getTriggeredAction(id: string) {
    return this.http.get<TriggeredActionDto>(`${getBaseUrl()}/job-monitor/triggered-action`, {
      params: {
        id,
      },
    });
  }

  createTriggeredAction(dto: CreateTriggeredActionDto) {
    return this.http.post<TriggeredActionDto>(`${getBaseUrl()}/job-monitor/triggered-action`, dto);
  }

  updateTriggeredAction(dto: UpdateTriggeredActionDto) {
    return this.http.put<TriggeredActionDto>(`${getBaseUrl()}/job-monitor/triggered-action`, dto);
  }

  deleteTriggeredAction(id: string) {
    return this.http.delete<void>(`${getBaseUrl()}/job-monitor/triggered-action`, {
      params: {
        id,
      },
    });
  }

  setTriggeredActionActive(id: string, isActive: boolean) {
    return this.http.post<void>(`${getBaseUrl()}/job-monitor/triggered-action/set-active`, null, {
      params: {
        id,
        active: isActive.toString(),
      },
    });
  }

  resetTriggeredAction(id: string) {
    return this.http.post<void>(`${getBaseUrl()}/job-monitor/triggered-action/reset`, null, {
      params: {
        id,
      },
    });
  }
}
