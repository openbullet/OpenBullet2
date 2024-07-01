import { Component, HostListener, ViewChild } from '@angular/core';
import { faCode, faGear, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { EnvironmentSettingsDto } from 'src/app/main/dtos/settings/environment-settings.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { CodeEditorComponent } from 'src/app/shared/components/code-editor/code-editor.component';

@Component({
  selector: 'app-config-loliscript',
  templateUrl: './config-loliscript.component.html',
  styleUrls: ['./config-loliscript.component.scss'],
})
export class ConfigLoliscriptComponent {
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

  @ViewChild('editor')
  editor: CodeEditorComponent | undefined = undefined;

  constructor(
    private configService: ConfigService,
    private settingsService: SettingsService,
    private messageService: MessageService,
  ) {
    this.configService.selectedConfig$.subscribe((config) => {
      this.config = config;
    });
  }

  editorLoaded() {
    if (this.editor !== undefined && this.config !== null) {
      this.editor.code = this.config.loliScript;
    }
  }

  codeChanged(code: string) {
    if (this.config !== null) {
      this.config.loliScript = code;
      this.localSave();
    }
  }

  ngOnInit(): void {
    this.settingsService.getEnvironmentSettings().subscribe((envSettings) => {
      this.envSettings = envSettings;
    });
  }

  localSave() {
    if (this.config !== null) {
      this.configService.saveLocalConfig(this.config);
    }
  }
}
