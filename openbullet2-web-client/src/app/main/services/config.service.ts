import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { getBaseUrl } from 'src/app/shared/utils/host';
import { AffectedEntriesDto } from '../dtos/common/affected-entries.dto';
import { BlockDescriptors } from '../dtos/config/block-descriptor.dto';
import { BlockInstanceTypes } from '../dtos/config/block-instance.dto';
import { CategoryTreeNodeDto } from '../dtos/config/category-tree.dto';
import { ConfigInfoDto } from '../dtos/config/config-info.dto';
import { ConfigReadmeDto } from '../dtos/config/config-readme.dto';
import { ConfigDto, ConfigSettingsDto } from '../dtos/config/config.dto';
import { ConvertedCSharpDto, ConvertedLoliCodeDto, ConvertedStackDto } from '../dtos/config/conversion.dto';
import { UpdateConfigDto } from '../dtos/config/update-config.dto';
import * as CryptoJS from 'crypto-js';

interface ConfigHashInfo {
  id: string;
  hash: string;
}

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private selectedConfigSource = new BehaviorSubject<ConfigDto | null>(null);
  selectedConfig$ = this.selectedConfigSource.asObservable();
  selectedConfig: ConfigDto | null = null;

  private nameChangedSource = new BehaviorSubject<string | null>(null);
  nameChanged$ = this.nameChangedSource.asObservable();

  constructor(private http: HttpClient) {
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
        // biome-ignore lint/suspicious/noExplicitAny: This is a valid use case for catching any error
      } catch (err: any) {
        console.log('Could not restore config from localStorage, maybe the interface changed?', err);
      }
    }
  }

  selectConfig(config: ConfigDto | null) {
    this.selectedConfigSource.next(config);
    this.selectedConfig = config;

    if (config !== null) {
      this.saveLocalConfig(config);
      this.saveConfigHash();
    } else {
      this.resetLocalConfig();
    }
  }

  saveConfigHash() {
    if (this.selectedConfig === null) {
      return;
    }

    const json = JSON.stringify(this.selectedConfig);
    const configHash = CryptoJS.SHA256(json).toString(CryptoJS.enc.Hex);
    const configHashInfo = <ConfigHashInfo>{ id: this.selectedConfig.id, hash: configHash };
    window.localStorage.setItem('configHash', JSON.stringify(configHashInfo));
  }

  // Checks if the current config was changed and not saved
  hasUnsavedChanges() {
    if (this.selectedConfig === null) {
      return false;
    }

    const json = JSON.stringify(this.selectedConfig);
    const configHash = CryptoJS.SHA256(json).toString(CryptoJS.enc.Hex);
    const existing = window.localStorage.getItem('configHash');

    if (existing === null) {
      return true;
    }

    const configHashInfo: ConfigHashInfo = JSON.parse(existing);

    // If it's another config, we cannot compare
    if (configHashInfo.id !== this.selectedConfig.id) {
      return false;
    }

    return configHash !== configHashInfo.hash;
  }

  nameChanged(label: string) {
    this.nameChangedSource.next(label);
  }

  getAllConfigs(reload: boolean) {
    return this.http.get<ConfigInfoDto[]>(`${getBaseUrl()}/config/all`, {
      params: {
        reload,
      },
    });
  }

  getReadme(id: string) {
    return this.http.get<ConfigReadmeDto>(`${getBaseUrl()}/config/readme`, {
      params: {
        id,
      },
    });
  }

  getMetadata(id: string) {
    return this.http.get<ConfigDto>(`${getBaseUrl()}/config/metadata`, {
      params: {
        id,
      },
    });
  }

  getInfo(id: string) {
    return this.http.get<ConfigInfoDto>(`${getBaseUrl()}/config/info`, {
      params: {
        id,
      },
    });
  }

  getConfig(id: string) {
    return this.http.get<ConfigDto>(`${getBaseUrl()}/config`, {
      params: {
        id,
      },
    });
  }

  saveConfig(config: ConfigDto, persistent: boolean) {
    const updated: UpdateConfigDto = {
      id: config.id,
      mode: config.mode,
      metadata: {
        name: config.metadata.name,
        category: config.metadata.category,
        author: config.metadata.author,
        base64Image: config.metadata.base64Image,
      },
      settings: config.settings,
      readme: config.readme,
      loliCodeScript: config.loliCodeScript,
      startupLoliCodeScript: config.startupLoliCodeScript,
      loliScript: config.loliScript,
      cSharpScript: config.cSharpScript,
      startupCSharpScript: config.startupCSharpScript,
      persistent,
    };

    return this.updateConfig(updated)
      .pipe(data => {
        this.saveConfigHash();
        return data;
      });
  }

  updateConfig(updated: UpdateConfigDto) {
    return this.http.put<ConfigDto>(`${getBaseUrl()}/config`, updated);
  }

  createConfig() {
    return this.http.post<ConfigDto>(`${getBaseUrl()}/config`, {});
  }

  deleteConfig(id: string) {
    return this.http.delete(`${getBaseUrl()}/config`, {
      params: {
        id,
      },
    });
  }

  cloneConfig(id: string) {
    return this.http.post<ConfigDto>(
      `${getBaseUrl()}/config/clone`,
      {},
      {
        params: {
          id,
        },
      },
    );
  }

  downloadConfig(id: string) {
    return this.http.get<Blob>(`${getBaseUrl()}/config/download`, {
      params: {
        id,
      },
      responseType: 'blob' as 'json',
      observe: 'response',
    });
  }

  downloadAllConfigs() {
    return this.http.get<Blob>(`${getBaseUrl()}/config/download/all`, {
      responseType: 'blob' as 'json',
      observe: 'response',
    });
  }

  uploadConfigs(files: File[]) {
    const formData: FormData = new FormData();
    for (const file of files) {
      formData.append('files', file, file.name);
    }
    return this.http.post<AffectedEntriesDto>(`${getBaseUrl()}/config/upload/many`, formData);
  }

  convertLoliCodeToCSharp(settings: ConfigSettingsDto, loliCode: string) {
    return this.http.post<ConvertedCSharpDto>(`${getBaseUrl()}/config/convert/lolicode/csharp`, {
      settings,
      loliCode,
    });
  }

  convertStackToLoliCode(stack: BlockInstanceTypes[]) {
    return this.http.post<ConvertedLoliCodeDto>(`${getBaseUrl()}/config/convert/stack/lolicode`, {
      stack,
    });
  }

  convertLoliCodeToStack(loliCode: string) {
    return this.http.post<ConvertedStackDto>(`${getBaseUrl()}/config/convert/lolicode/stack`, {
      loliCode,
    });
  }

  getBlockDescriptors(): Observable<BlockDescriptors> {
    return this.http.get<BlockDescriptors>(`${getBaseUrl()}/config/block-descriptors`);
  }

  getCategoryTree(): Observable<CategoryTreeNodeDto> {
    return this.http.get<CategoryTreeNodeDto>(`${getBaseUrl()}/config/category-tree`);
  }

  getBlockInstance(id: string): Observable<BlockInstanceTypes> {
    return this.http.get<BlockInstanceTypes>(`${getBaseUrl()}/config/block-instance`, {
      params: {
        id,
      },
    });
  }

  getBlockSnippets() {
    return this.http.get<{ [key: string]: string }>(`${getBaseUrl()}/config/block-snippets`);
  }

  getRemoteImage(url: string) {
    return this.http.get<Blob>(`${getBaseUrl()}/config/remote-image`, {
      params: {
        url
      },
      responseType: 'blob' as 'json'
    });
  }
}
