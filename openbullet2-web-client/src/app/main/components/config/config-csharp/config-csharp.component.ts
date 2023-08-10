import { Component, OnInit, ViewChild } from '@angular/core';
import { faCode, faGear, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { EnvironmentSettingsDto } from 'src/app/main/dtos/settings/environment-settings.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { LolicodeEditorComponent } from 'src/app/shared/components/code-editor/code-editor.component';

@Component({
  selector: 'app-config-csharp',
  templateUrl: './config-csharp.component.html',
  styleUrls: ['./config-csharp.component.scss']
})
export class ConfigCsharpComponent {
  envSettings: EnvironmentSettingsDto | null = null;
  config: ConfigDto | null = null;
  faTriangleExclamation = faTriangleExclamation;
  faGear = faGear;
  faCode = faCode;
  wordlistTypes: string[] = [];
  showUsings: boolean = false;

  @ViewChild('editor')
  editor: LolicodeEditorComponent | undefined = undefined;

  constructor(private configService: ConfigService,
    private settingsService: SettingsService) {
      this.configService.selectedConfig$
        .subscribe(config => this.config = config);
  }

  editorLoaded() {
    if (this.editor !== undefined && this.config !== null) {
      this.editor.code = this.config.cSharpScript;
    }
  }

  codeChanged(code: string) {
    if (this.config !== null) {
      this.config.cSharpScript = code;
      this.localSave();
    }
  }

  ngOnInit(): void {
    this.settingsService.getEnvironmentSettings()
      .subscribe(envSettings => {
        this.envSettings = envSettings;
        this.wordlistTypes = envSettings.wordlistTypes.map(w => w.name);
      });
  }

  localSave() {
    if (this.config !== null) {
      this.configService.saveLocalConfig(this.config);
    }
  }

  toggleUsings() {
    this.showUsings = !this.showUsings;
  }
}
