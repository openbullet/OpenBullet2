import { Component, HostListener, ViewChild } from '@angular/core';
import { faCode, faGear, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
import { ConfigMode } from 'src/app/main/dtos/config/config-info.dto';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { EnvironmentSettingsDto } from 'src/app/main/dtos/settings/environment-settings.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { CodeEditorComponent } from 'src/app/shared/components/code-editor/code-editor.component';

@Component({
  selector: 'app-config-csharp',
  templateUrl: './config-csharp.component.html',
  styleUrls: ['./config-csharp.component.scss'],
})
export class ConfigCsharpComponent {
  // Listen for CTRL+S on the page
  @HostListener('document:keydown.control.s', ['$event'])
  onKeydownHandler(event: KeyboardEvent) {
    event.preventDefault();

    if (this.config !== null) {
      this.configService.saveConfig(this.config, true).subscribe((c) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Saved',
          detail: `${c.metadata.name} was saved`,
        });
      });
    }
  }

  envSettings: EnvironmentSettingsDto | null = null;
  config: ConfigDto | null = null;
  faTriangleExclamation = faTriangleExclamation;
  faGear = faGear;
  faCode = faCode;
  wordlistTypes: string[] = [];
  showUsings = false;
  showStartupEditor = false;
  ConfigMode = ConfigMode;

  @ViewChild('editor')
  editor: CodeEditorComponent | undefined = undefined;

  @ViewChild('startupEditor')
  startupEditor: CodeEditorComponent | undefined = undefined;

  constructor(
    private configService: ConfigService,
    private settingsService: SettingsService,
    private messageService: MessageService,
  ) {
    this.configService.selectedConfig$.subscribe((config) => {
      this.config = config;
      if (this.config?.startupCSharpScript !== '') {
        this.showStartupEditor = true;
      }
    });
  }

  editorLoaded() {
    if (this.editor !== undefined && this.config !== null) {
      this.editor.code = this.config.cSharpScript;
    }
  }

  startupEditorLoaded() {
    if (this.startupEditor !== undefined && this.config !== null) {
      this.startupEditor.code = this.config.startupCSharpScript;
    }
  }

  codeChanged(code: string) {
    if (this.config !== null) {
      this.config.cSharpScript = code;
      this.localSave();
    }
  }

  startupCodeChanged(code: string) {
    if (this.config !== null) {
      this.config.startupCSharpScript = code;
      this.localSave();
    }
  }

  ngOnInit(): void {
    this.settingsService.getEnvironmentSettings().subscribe((envSettings) => {
      this.envSettings = envSettings;
      this.wordlistTypes = envSettings.wordlistTypes.map((w) => w.name);
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

  toggleStartupEditor() {
    this.showStartupEditor = !this.showStartupEditor;
  }
}
