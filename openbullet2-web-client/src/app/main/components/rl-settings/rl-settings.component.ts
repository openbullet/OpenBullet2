import { Component, HostListener, OnInit } from '@angular/core';
import { RLSettingsDto } from '../../dtos/settings/rl-settings.dto';
import { SettingsService } from '../../services/settings.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FieldValidity } from 'src/app/shared/utils/forms';

@Component({
  selector: 'app-rl-settings',
  templateUrl: './rl-settings.component.html',
  styleUrls: ['./rl-settings.component.scss']
})
export class RlSettingsComponent implements OnInit {
  // TODO: Add a guard as well so if the user navigates away
  // from the page using the router it will also prompt the warning!
  @HostListener('window:beforeunload') confirmLeavingWithoutSaving(): boolean {
    return !this.touched;
  }

  fieldsValidity: { [key: string] : boolean; } = {};
  settings: RLSettingsDto | null = null;
  touched: boolean = false;
  parallelizerTypes: string[] = [
    'taskBased',
    'threadBased',
    'parallelBased'
  ];
  browserTypes: string[] = [
    'chrome',
    'firefox'
  ];
  captchaServiceTypes: string[] = [
    'twoCaptcha',
    'antiCaptcha',
    'customTwoCaptcha',
    'deathByCaptcha',
    'deCaptcher',
    'imageTyperz',
    'capMonster',
    'aZCaptcha',
    'captchasIO',
    'ruCaptcha',
    'solveCaptcha',
    'solveRecaptcha',
    'trueCaptcha',
    'nineKW',
    'customAntiCaptcha',
    'anyCaptcha',
    'capSolver'
  ];

  constructor(private settingsService: SettingsService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService) {}

  ngOnInit(): void {
    this.getSettings();
  }

  getSettings() {
    this.settingsService.getRuriLibSettings()
      .subscribe(settings => this.settings = settings);
  }

  saveSettings() {
    if (this.settings === null) return;
    this.settingsService.updateRuriLibSettings(this.settings)
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
    this.settingsService.getDefaultRuriLibSettings()
      .subscribe(defaultSettings => {
        this.settingsService.updateRuriLibSettings(defaultSettings)
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

  onCaptchaServiceChange(newValue: string) {
    this.settings!.captchaSettings.currentService = newValue;
  }
}
