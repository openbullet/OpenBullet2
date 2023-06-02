import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { getBaseUrl } from "src/app/shared/utils/host";
import { OBSettingsDto } from "../dtos/settings/ob-settings.dto";
import { RLSettingsDto } from "../dtos/settings/rl-settings.dto";
import { EnvironmentSettingsDto } from "../dtos/settings/environment-settings.dto";

@Injectable({
    providedIn: 'root'
})
export class SettingsService {
    constructor(
        private http: HttpClient
    ) { }

    getEnvironmentSettings() {
        return this.http.get<EnvironmentSettingsDto>(
            getBaseUrl() + '/settings/environment'
        );
    }

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

    getRuriLibSettings() {
        return this.http.get<RLSettingsDto>(
            getBaseUrl() + '/settings/rurilib'
        );
    }

    getDefaultRuriLibSettings() {
        return this.http.get<RLSettingsDto>(
            getBaseUrl() + '/settings/rurilib/default'
        );
    }

    updateSettings(updated: OBSettingsDto) {
        return this.http.put<OBSettingsDto>(
            getBaseUrl() + '/settings', updated
        );
    }

    updateRuriLibSettings(updated: RLSettingsDto) {
        return this.http.put<RLSettingsDto>(
            getBaseUrl() + '/settings/rurilib', updated
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
