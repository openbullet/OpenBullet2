import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AnnouncementDto } from "../dtos/info/announcement.dto";
import { getBaseUrl } from "src/app/shared/utils/host";
import { ServerInfoDto } from "../dtos/info/server-info.dto";
import { CollectionInfoDto } from "../dtos/info/collection-info.dto";
import { PerformanceInfoDto } from "../dtos/info/performance-info.dto";

@Injectable({
    providedIn: 'root'
})
export class InfoService {
    constructor(
        private http: HttpClient
    ) { }

    getAnnouncement() {
        return this.http.get<AnnouncementDto>(
            getBaseUrl() + '/info/announcement'
            // TODO: Add auth header!
        );
    }

    getServerInfo() {
        return this.http.get<ServerInfoDto>(
            getBaseUrl() + '/info/server'
        );
    }

    getCollectionInfo() {
        return this.http.get<CollectionInfoDto>(
            getBaseUrl() + '/info/collection'
        );
    }
}
