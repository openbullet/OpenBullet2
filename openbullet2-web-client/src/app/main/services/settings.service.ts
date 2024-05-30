import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { getBaseUrl } from 'src/app/shared/utils/host';
import { EnvironmentSettingsDto } from '../dtos/settings/environment-settings.dto';
import { OBSettingsDto, SafeOBSettingsDto } from '../dtos/settings/ob-settings.dto';
import { CaptchaRLSettings, RLSettingsDto } from '../dtos/settings/rl-settings.dto';
import { ThemeDto } from '../dtos/settings/theme.dto';
import { CaptchaBalanceDto } from '../dtos/settings/captcha-balance.dto';
import { SystemSettingsDto } from '../dtos/settings/system-settings.dto';

@Injectable({
  providedIn: 'root',
})
export class SettingsService {
  // Cached
  private envSettings$: Observable<EnvironmentSettingsDto> | null = null;
  private systemSettings$: Observable<SystemSettingsDto> | null = null;

  constructor(private http: HttpClient) { }

  getSystemSettings() {
    if (this.systemSettings$ !== null) {
      return this.systemSettings$;
    }

    this.systemSettings$ = this.http
      .get<SystemSettingsDto>(`${getBaseUrl()}/settings/system`)
      .pipe(shareReplay(1));

    return this.systemSettings$;
  }

  getEnvironmentSettings() {
    if (this.envSettings$ !== null) {
      return this.envSettings$;
    }

    this.envSettings$ = this.http
      .get<EnvironmentSettingsDto>(`${getBaseUrl()}/settings/environment`)
      .pipe(shareReplay(1));

    return this.envSettings$;
  }

  getSettings() {
    return this.http.get<OBSettingsDto>(`${getBaseUrl()}/settings`);
  }

  getDefaultSettings() {
    return this.http.get<OBSettingsDto>(`${getBaseUrl()}/settings/default`);
  }

  getSafeSettings() {
    return this.http.get<SafeOBSettingsDto>(`${getBaseUrl()}/settings/safe`);
  }

  getRuriLibSettings() {
    return this.http.get<RLSettingsDto>(`${getBaseUrl()}/settings/rurilib`);
  }

  getDefaultRuriLibSettings() {
    return this.http.get<RLSettingsDto>(`${getBaseUrl()}/settings/rurilib/default`);
  }

  updateSettings(updated: OBSettingsDto) {
    return this.http.put<OBSettingsDto>(`${getBaseUrl()}/settings`, updated);
  }

  updateRuriLibSettings(updated: RLSettingsDto) {
    return this.http.put<RLSettingsDto>(`${getBaseUrl()}/settings/rurilib`, updated);
  }

  updateAdminPassword(password: string) {
    return this.http.patch(`${getBaseUrl()}/settings/admin/password`, {
      password,
    });
  }

  uploadTheme(file: File) {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post(`${getBaseUrl()}/settings/theme`, formData);
  }

  getAllThemes() {
    return this.http.get<ThemeDto[]>(`${getBaseUrl()}/settings/theme/all`);
  }

  getTheme(name: string | null) {
    return this.http.get<string>(`${getBaseUrl()}/settings/theme`, {
      params: name === null ? {} : { name },
    });
  }

  getCustomSnippets() {
    return this.http.get<{ [key: string]: string }>(`${getBaseUrl()}/settings/custom-snippets`);
  }

  checkCaptchaBalance(dto: CaptchaRLSettings) {
    return this.http.post<CaptchaBalanceDto>(
      `${getBaseUrl()}/settings/check-captcha-balance`, dto);
  }
}
