import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getBaseUrl } from 'src/app/shared/utils/host';
import { PluginDto } from '../dtos/plugin/plugin.dto';

@Injectable({
  providedIn: 'root',
})
export class PluginService {
  constructor(private http: HttpClient) {}

  getAllPlugins() {
    return this.http.get<PluginDto[]>(`${getBaseUrl()}/plugin/all`);
  }

  addPlugin(file: File) {
    const formData: FormData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post(`${getBaseUrl()}/plugin`, formData);
  }

  deletePlugin(name: string) {
    return this.http.delete(`${getBaseUrl()}/plugin`, {
      params: {
        name,
      },
    });
  }
}
