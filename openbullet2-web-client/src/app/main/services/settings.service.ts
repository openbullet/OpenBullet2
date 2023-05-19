import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { getBaseUrl } from "src/app/shared/utils/host";
import { OBSettingsDto } from "../dtos/settings/ob-settings.dto";

@Injectable({
    providedIn: 'root'
})
export class SettingsService {
    constructor(
        private http: HttpClient
    ) { }

    getSettings() {
        return this.http.get<OBSettingsDto>(
            getBaseUrl() + '/settings'
        );
    }

    getDefaultSettings() {
        return this.http.get<OBSettingsDto>(
            getBaseUrl() + '/settings/default'
        );
    }

    updateSettings(updated: OBSettingsDto) {
        return this.http.put<OBSettingsDto>(
            getBaseUrl() + '/settings', updated
        );
    }

    updateAdminPassword(password: string) {
        return this.http.patch(
            getBaseUrl() + '/settings/admin/password',
            {
                password
            }
        )
    }
}
