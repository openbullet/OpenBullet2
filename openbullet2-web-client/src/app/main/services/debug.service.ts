import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getBaseUrl } from 'src/app/shared/utils/host';

@Injectable({
  providedIn: 'root',
})
export class DebugService {
  constructor(private http: HttpClient) { }

  garbageCollect() {
    return this.http.post(`${getBaseUrl()}/debug/gc`, {
      generations: -1,
      mode: 'aggressive',
      blocking: true,
      compacting: true,
    });
  }

  downloadLogFile() {
    return this.http.get<Blob>(`${getBaseUrl()}/debug/server-logs`, {
      responseType: 'blob' as 'json',
      observe: 'response',
    });
  }
}
