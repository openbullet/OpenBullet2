import { Injectable } from '@angular/core';
import { ConfigDebuggerSettings } from '../models/config-debugger-settings';

@Injectable({
  providedIn: 'root',
})
export class ConfigDebuggerSettingsService {
  resetLocalSettings() {
    window.localStorage.removeItem('config-debugger-settings');
  }

  saveLocalSettings(settings: ConfigDebuggerSettings) {
    window.localStorage.setItem('config-debugger-settings', JSON.stringify(settings));
  }

  loadLocalSettings(): ConfigDebuggerSettings {
    const json = window.localStorage.getItem('config-debugger-settings');
    let settings: ConfigDebuggerSettings | null = null;

    if (json !== null) {
      try {
        settings = JSON.parse(json);
      } catch { }
    }

    return (
      settings ?? {
        testData: '',
        wordlistType: 'Default',
        useProxy: false,
        testProxy: '',
        proxyType: 'http',
        persistLog: false,
        stepByStep: false,
        groupCaptures: false
      }
    );
  }
}
