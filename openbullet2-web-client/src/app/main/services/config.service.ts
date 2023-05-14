import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { getBaseUrl } from "src/app/shared/utils/host";
import { ConfigInfoDto } from "../dtos/config/config-info.dto";

@Injectable({
    providedIn: 'root'
})
export class ConfigService {
    constructor(
        private http: HttpClient
    ) { }

    getAllConfigs() {
        return this.http.get<ConfigInfoDto[]>(
            getBaseUrl() + '/config/all'
        );
    }
}
