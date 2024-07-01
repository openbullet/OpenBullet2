import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getBaseUrl } from 'src/app/shared/utils/host';
import { AnnouncementDto } from '../dtos/info/announcement.dto';
import { ChangelogDto } from '../dtos/info/changelog.dto';
import { CollectionInfoDto } from '../dtos/info/collection-info.dto';
import { ServerInfoDto } from '../dtos/info/server-info.dto';
import { UpdateInfoDto } from '../dtos/info/update-info.dto';

@Injectable({
  providedIn: 'root',
})
export class InfoService {
  constructor(private http: HttpClient) {}

  getAnnouncement() {
    return this.http.get<AnnouncementDto>(`${getBaseUrl()}/info/announcement`);
  }

  getServerInfo() {
    return this.http.get<ServerInfoDto>(`${getBaseUrl()}/info/server`);
  }

  getCollectionInfo() {
    return this.http.get<CollectionInfoDto>(`${getBaseUrl()}/info/collection`);
  }

  getChangelog(version: string | null) {
    return this.http.get<ChangelogDto>(`${getBaseUrl()}/info/changelog${version ? `?v=${version}` : ''}`);
  }

  getUpdateInfo() {
    return this.http.get<UpdateInfoDto>(`${getBaseUrl()}/info/update`);
  }
}
