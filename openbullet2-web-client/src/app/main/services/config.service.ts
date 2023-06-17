import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { getBaseUrl } from "src/app/shared/utils/host";
import { ConfigInfoDto } from "../dtos/config/config-info.dto";
import { ConfigReadmeDto } from "../dtos/config/config-readme.dto";
import { ConfigDto } from "../dtos/config/config.dto";
import { UpdateConfigDto } from "../dtos/config/update-config.dto";
import { AffectedEntriesDto } from "../dtos/common/affected-entries.dto";
import { BehaviorSubject } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class ConfigService {
    private selectedConfigSource = new BehaviorSubject<ConfigDto | null>(null);
    selectedConfig$ = this.selectedConfigSource.asObservable();

    constructor(
        private http: HttpClient
    ) {
        // If there is a config saved in the localstorage, load it back in
        if (window.localStorage.getItem('config') !== null) {
            this.loadLocalConfig();
        }
    }

    resetLocalConfig() {
        window.localStorage.removeItem('config');
    }

    saveLocalConfig(config: ConfigDto) {
        window.localStorage.setItem('config', JSON.stringify(config));
    }

    loadLocalConfig() {
        const json = window.localStorage.getItem('config');

        if (json !== null) {
            try {
                const config: ConfigDto = JSON.parse(json);
                this.selectConfig(config);
            } catch (err: any) {
                console.log('Could not restore config from localStorage, maybe the interface changed?', err);
            }
        }
    }

    selectConfig(config: ConfigDto | null) {
        this.selectedConfigSource.next(config);

        if (config !== null) {
            this.saveLocalConfig(config);
        } else {
            this.resetLocalConfig();
        }
    }

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

    saveConfig(config: ConfigDto) {
        const updated: UpdateConfigDto = {
            id: config.id,
            mode: config.mode,
            metadata: {
                name: config.metadata.name,
                category: config.metadata.category,
                author: config.metadata.author,
                base64Image: config.metadata.base64Image
            },
            settings: config.settings,
            readme: config.readme,
            loliCodeScript: config.loliCodeScript,
            startupLoliCodeScript: config.startupLoliCodeScript,
            loliScript: config.loliScript,
            cSharpScript: config.cSharpScript,
            startupCSharpScript: config.startupCSharpScript
        };

        return this.updateConfig(updated);
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
            formData.append('files', file, file.name);
        }
        return this.http.post<AffectedEntriesDto>(
            getBaseUrl() + '/config/upload/many', formData
        );
    }
}
