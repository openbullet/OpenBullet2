import { HttpClient, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { getBaseUrl } from "src/app/shared/utils/host";
import { ProxyGroupDto } from "../dtos/proxy-group/proxy-group.dto";
import { UpdateProxyGroupDto } from "../dtos/proxy-group/update-proxy-group.dto";
import { CreateProxyGroupDto } from "../dtos/proxy-group/create-proxy-group.dto";
import { ProxyFiltersDto } from "../dtos/proxy/proxy-filter.dto";
import { ProxyDto } from "../dtos/proxy/proxy.dto";
import { AffectedEntriesDto } from "../dtos/common/affected-entries.dto";
import { AddProxiesFromListDto } from "../dtos/proxy/add-proxies-from-list.dto";
import { AddProxiesFromRemoteDto } from "../dtos/proxy/add-proxies-from-remote.dto";
import { MoveProxiesDto } from "../dtos/proxy/move-proxies.dto";
import { PagedList } from "../dtos/common/paged-list.dto";
import { Observable, map } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class ProxyService {
    constructor(
        private http: HttpClient
    ) { }

    getProxies(filter: ProxyFiltersDto) {
        return this.http.get<PagedList<ProxyDto>>(
            getBaseUrl() + '/proxy/all', {
            params: <any>Object.fromEntries(
                Object.entries(filter).filter(([_, v]) => v != null))
        }
        );
    }

    addProxiesFromList(list: AddProxiesFromListDto) {
        return this.http.post<AffectedEntriesDto>(
            getBaseUrl() + '/proxy/add', list
        );
    }

    addProxiesFromRemote(remote: AddProxiesFromRemoteDto) {
        return this.http.post<AffectedEntriesDto>(
            getBaseUrl() + '/proxy/add-from-remote', remote
        );
    }

    moveProxies(info: MoveProxiesDto) {
        return this.http.post<AffectedEntriesDto>(
            getBaseUrl() + '/proxy/move/many',
            <any>Object.fromEntries(
                Object.entries(info).filter(([_, v]) => v != null))
        );
    }

    downloadProxies(filter: ProxyFiltersDto) {
        return this.http.get<Blob>(
            getBaseUrl() + '/proxy/download/many', {
            params: <any>Object.fromEntries(
                Object.entries(filter).filter(([_, v]) => v != null)),
            responseType: 'blob' as 'json',
            observe: 'response'
        }
        );
    }

    deleteProxies(filter: ProxyFiltersDto) {
        return this.http.delete<AffectedEntriesDto>(
            getBaseUrl() + '/proxy/many', {
            params: <any>Object.fromEntries(
                Object.entries(filter).filter(([_, v]) => v != null))
        }
        );
    }

    deleteSlowProxies(proxyGroupId: number, maxPing: number) {
        return this.http.delete<AffectedEntriesDto>(
            getBaseUrl() + '/proxy/slow', {
            params: {
                proxyGroupId,
                maxPing
            }
        }
        );
    }
}
