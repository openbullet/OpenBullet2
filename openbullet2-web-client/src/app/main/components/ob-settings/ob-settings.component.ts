import { Component, HostListener, OnInit } from '@angular/core';
import { SettingsService } from '../../services/settings.service';
import { OBSettingsDto } from '../../dtos/settings/ob-settings.dto';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FieldValidity } from 'src/app/shared/utils/forms';

@Component({
  selector: 'app-ob-settings',
  templateUrl: './ob-settings.component.html',
  styleUrls: ['./ob-settings.component.scss']
})
export class OBSettingsComponent implements OnInit {
  // TODO: Add a guard as well so if the user navigates away
  // from the page using the router it will also prompt the warning!
  @HostListener('window:beforeunload') confirmLeavingWithoutSaving(): boolean {
    return !this.touched;
  }

  fieldsValidity: { [key: string] : boolean; } = {};
  settings: OBSettingsDto | null = null;
  touched: boolean = false;
  configSections: string[] = [
    'metadata',
    'readme',
    'stacker',
    'loliCode',
    'settings',
    'cSharpCode',
    'loliScript'
  ];

  constructor(private settingsService: SettingsService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService) {}
  
  ngOnInit(): void {
    this.getSettings();
  }

  getSettings() {
    this.settingsService.getSettings()
      .subscribe(settings => this.settings = settings);
  }

  saveSettings() {
    if (this.settings === null) return;
    this.settingsService.updateSettings(this.settings)
      .subscribe(settings => {
        this.messageService.add({
          severity: 'success',
          summary: 'Saved',
          detail: 'The settings were successfully saved'
        });
        this.touched = false;
        this.settings = settings;
      });
  }

  confirmRestoreDefaults() {
    this.confirmationService.confirm({
      message: `You are about to restore the default settings. 
      Are you sure that you want to proceed?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.restoreDefaults()
    });
  }

  restoreDefaults() {
    this.settings = null;
    this.settingsService.getDefaultSettings()
      .subscribe(defaultSettings => {
        this.settingsService.updateSettings(defaultSettings)
          .subscribe(settings => {
            this.messageService.add({
              severity: 'success',
              summary: 'Restored',
              detail: 'Settings restored to the default values'
            });
            this.settings = settings;
          })
      });
  }

  onValidityChange(validity: FieldValidity) {
    this.fieldsValidity[validity.key] = validity.valid;
  }

  // Can save if touched and every field is valid
  canSave() {
    return this.touched && Object.values(this.fieldsValidity).every(v => v);
  }
}
