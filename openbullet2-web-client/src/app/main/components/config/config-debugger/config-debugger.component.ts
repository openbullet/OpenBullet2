import { Component, Input, OnInit } from '@angular/core';
import { faPlay } from '@fortawesome/free-solid-svg-icons';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { EnvironmentSettingsDto } from 'src/app/main/dtos/settings/environment-settings.dto';
import { ConfigDebuggerSettings } from 'src/app/main/models/config-debugger-settings';
import { ConfigDebuggerService } from 'src/app/main/services/config-debugger.service';

@Component({
  selector: 'app-config-debugger',
  templateUrl: './config-debugger.component.html',
  styleUrls: ['./config-debugger.component.scss']
})
export class ConfigDebuggerComponent implements OnInit {
  @Input() config!: ConfigDto;
  @Input() envSettings!: EnvironmentSettingsDto;
  settings: ConfigDebuggerSettings | null = null;
  wordlistTypes: string[] = [
    'Default'
  ];
  proxyTypes: string[] = [
    'http',
    'socks4',
    'socks4a',
    'socks5'
  ];

  faPlay = faPlay;

  constructor (private debuggerSettingsService: ConfigDebuggerService) {

  }

  ngOnInit() {
    this.wordlistTypes = this.envSettings.wordlistTypes.map(t => t.name);
    this.settings = this.debuggerSettingsService.loadLocalSettings();
  }

  start() {
    console.log('start');
  }

  localSave() {
    if (this.settings !== null) {
      this.debuggerSettingsService.saveLocalSettings(this.settings);
    }
  }
}
