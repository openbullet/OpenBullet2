import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class VolatileSettingsService {
  public get configsDisplayMode(): string {
    // grid | table
    return window.localStorage.getItem('configsDisplayMode') ?? 'grid';
  }

  public set configsDisplayMode(mode: string) {
    window.localStorage.setItem('configsDisplayMode', mode);
  }

  public get recentlyUsedBlockIds(): string[] {
    return JSON.parse(window.localStorage.getItem('recentlyUsedBlockIds') ?? '[]');
  }

  public set recentlyUsedBlockIds(ids: string[]) {
    window.localStorage.setItem('recentlyUsedBlockIds', JSON.stringify(ids));
  }
}
