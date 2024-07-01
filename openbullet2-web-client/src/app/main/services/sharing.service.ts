import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getBaseUrl } from 'src/app/shared/utils/host';
import { EndpointDto } from '../dtos/sharing/endpoint.dto';

@Injectable({
  providedIn: 'root',
})
export class SharingService {
  constructor(private http: HttpClient) {}

  getAllEndpoints() {
    return this.http.get<EndpointDto[]>(`${getBaseUrl()}/shared/endpoint/all`);
  }

  createEndpoint(endpoint: EndpointDto) {
    return this.http.post<EndpointDto>(`${getBaseUrl()}/shared/endpoint`, endpoint);
  }

  updateEndpoint(updated: EndpointDto) {
    return this.http.put<EndpointDto>(`${getBaseUrl()}/shared/endpoint`, updated);
  }

  deleteEndpoint(route: string) {
    return this.http.delete(`${getBaseUrl()}/shared/endpoint`, {
      params: {
        route,
      },
    });
  }
}
