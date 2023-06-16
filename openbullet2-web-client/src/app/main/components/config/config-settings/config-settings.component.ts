import { Component, OnInit } from '@angular/core';
import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { EnvironmentSettingsDto } from 'src/app/main/dtos/settings/environment-settings.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { SettingsService } from 'src/app/main/services/settings.service';

@Component({
  selector: 'app-config-settings',
  templateUrl: './config-settings.component.html',
  styleUrls: ['./config-settings.component.scss']
})
export class ConfigSettingsComponent implements OnInit {
  envSettings: EnvironmentSettingsDto | null = null;
  config: ConfigDto | null = null;
  faTriangleExclamation = faTriangleExclamation;
  editImageModalVisible = false;

  botStatuses: string[] = [];

  constructor(private configService: ConfigService,
    private settingsService: SettingsService) {
    this.configService.selectedConfig$
      .subscribe(config => this.config = config);
  }

  ngOnInit(): void {
    this.settingsService.getEnvironmentSettings()
      .subscribe(envSettings => {
        this.envSettings = envSettings;
        this.botStatuses = [
          'SUCCESS',
          'NONE',
          'FAIL',
          'RETRY',
          'BAN',
          'ERROR',
          ...envSettings.customStatuses.map(s => s.name)
        ];
      });
  }

  updateMultiSelectorValues() {

  }

  localSave() {
    if (this.config !== null) {
      this.configService.saveLocalConfig(this.config);
    }
  }
}
