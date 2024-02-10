import { Component, HostListener, OnInit } from '@angular/core';
import { RLSettingsDto } from '../../dtos/settings/rl-settings.dto';
import { SettingsService } from '../../services/settings.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { faWrench } from '@fortawesome/free-solid-svg-icons';
import { DeactivatableComponent } from 'src/app/shared/guards/can-deactivate-form.guard';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-rl-settings',
  templateUrl: './rl-settings.component.html',
  styleUrls: ['./rl-settings.component.scss']
})
export class RlSettingsComponent implements OnInit, DeactivatableComponent {
  @HostListener('window:beforeunload') confirmLeavingWithoutSaving(): boolean {
    return !this.touched;
  }

  // Listen for CTRL+S on the page
  @HostListener('document:keydown.control.s', ['$event'])
  onKeydownHandler(event: KeyboardEvent) {
    event.preventDefault();

    if (!this.touched) {
      return;
    }

    if (this.canSave()) {
      this.saveSettings();
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Some fields are invalid, please fix them before saving'
      });
    }
  }

  fieldsValidity: { [key: string]: boolean; } = {};
  settings: RLSettingsDto | null = null;
  touched: boolean = false;
  faWrench = faWrench;
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
    private messageService: MessageService) { }

  canDeactivate() {
    if (!this.touched) {
      return true;
    }

    // Ask for confirmation and return the observable
    return new Observable<boolean>(observer => {
      this.confirmationService.confirm({
        message: `You have unsaved changes. Are you sure that you want to leave?`,
        header: 'Confirmation',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          observer.next(true);
          observer.complete();
        },
        reject: () => {
          observer.next(false);
          observer.complete();
        }
      });
    });
  }

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
    this.fieldsValidity = {
      ...this.fieldsValidity,
      [validity.key]: validity.valid
    };
  }

  // Can save if touched and every field is valid
  canSave() {
    return this.touched && Object.values(this.fieldsValidity).every(v => v);
  }

  onCaptchaServiceChange(newValue: string) {
    this.settings!.captchaSettings.currentService = newValue;
  }
}
