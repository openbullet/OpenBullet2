import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { getBaseUrl } from "src/app/shared/utils/host";
import { ConfigInfoDto } from "../dtos/config/config-info.dto";
import { ConfigReadmeDto } from "../dtos/config/config-readme.dto";
import { ConfigDto } from "../dtos/config/config.dto";
import { UpdateConfigDto } from "../dtos/config/update-config.dto";
import { AffectedEntriesDto } from "../dtos/common/affected-entries.dto";

@Injectable({
    providedIn: 'root'
})
export class ConfigService {
    constructor(
        private http: HttpClient
    ) { }

    getAllConfigs(reload: boolean) {
        return this.http.get<ConfigInfoDto[]>(
            getBaseUrl() + '/config/all', {
                params: {
                    reload
                }
            }
        );
    }

    getReadme(id: string) {
        return this.http.get<ConfigReadmeDto>(
            getBaseUrl() + '/config/readme',
            {
                params: {
                    id
                }
            }
        );
    }

    getConfig(id: string) {
        return this.http.get<ConfigDto>(
            getBaseUrl() + '/config',
            {
                params: {
                    id
                }
            }
        );
    }

    updateConfig(updated: UpdateConfigDto) {
        return this.http.put<ConfigDto>(
            getBaseUrl() + '/config', updated
        );
    }

    createConfig() {
        return this.http.post<ConfigDto>(
            getBaseUrl() + '/config', {}
        );
    }

    deleteConfig(id: string) {
        return this.http.delete(
            getBaseUrl() + '/config',
            {
                params: {
                    id
                }
            }
        );
    }

    cloneConfig(id: string) {
        return this.http.post<ConfigDto>(
            getBaseUrl() + '/config/clone', {}, {
                params: {
                    id
                }
            }
        );
    }

    downloadConfig(id: string) {
        return this.http.get<Blob>(
            getBaseUrl() + '/config/download', {
                params: {
                    id
                },
                responseType: 'blob' as 'json',
                observe: 'response'
            }
        );
    }

    downloadAllConfigs() {
        return this.http.get<Blob>(
            getBaseUrl() + '/config/download/all', {
                responseType: 'blob' as 'json',
                observe: 'response'
            }
        );
    }

    uploadConfigs(files: File[]) {
        const formData: FormData = new FormData();
        for (let file of files) {
            formData.append('file', file, file.name);
        }
        return this.http.post<AffectedEntriesDto>(
            getBaseUrl() + '/config/upload/many', formData, {
                reportProgress: true,
                observe: 'events'
            }
        );
    }
}
