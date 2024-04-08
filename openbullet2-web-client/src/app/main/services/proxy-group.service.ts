import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getBaseUrl } from 'src/app/shared/utils/host';
import { CreateProxyGroupDto } from '../dtos/proxy-group/create-proxy-group.dto';
import { ProxyGroupDto } from '../dtos/proxy-group/proxy-group.dto';
import { UpdateProxyGroupDto } from '../dtos/proxy-group/update-proxy-group.dto';

@Injectable({
  providedIn: 'root',
})
export class ProxyGroupService {
  constructor(private http: HttpClient) {}

  getAllProxyGroups() {
    return this.http.get<ProxyGroupDto[]>(`${getBaseUrl()}/proxy-group/all`);
  }

  createProxyGroup(guest: CreateProxyGroupDto) {
    return this.http.post<ProxyGroupDto>(`${getBaseUrl()}/proxy-group`, guest);
  }

  updateProxyGroup(updated: UpdateProxyGroupDto) {
    return this.http.put<ProxyGroupDto>(`${getBaseUrl()}/proxy-group`, updated);
  }

  deleteProxyGroup(id: number) {
    return this.http.delete(`${getBaseUrl()}/proxy-group`, {
      params: {
        id,
      },
    });
  }
}
