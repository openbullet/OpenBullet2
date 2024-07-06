import { Component, ElementRef, HostListener, OnInit, ViewChild } from '@angular/core';
import { faLink, faPen, faPlus, faUpRightFromSquare, faVolumeUp, faWrench, faX } from '@fortawesome/free-solid-svg-icons';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Observable } from 'rxjs';
import { DeactivatableComponent } from 'src/app/shared/guards/can-deactivate-form.guard';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { applyAppTheme } from 'src/app/shared/utils/theme';
import {
  ConfigSection,
  CustomSnippet,
  JobDisplayMode,
  OBSettingsDto,
  ProxyCheckTarget,
  RemoteConfigsEndpoint,
} from '../../dtos/settings/ob-settings.dto';
import { SettingsService } from '../../services/settings.service';

@Component({
  selector: 'app-ob-settings',
  templateUrl: './ob-settings.component.html',
  styleUrls: ['./ob-settings.component.scss'],
})
export class OBSettingsComponent implements OnInit, DeactivatableComponent {
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
        detail: 'Some fields are invalid, please fix them before saving',
      });
    }
  }

  @ViewChild('hitSoundPlayer') hitSoundPlayer!: ElementRef;

  faPlus = faPlus;
  faX = faX;
  faPen = faPen;
  faLink = faLink;
  faUpRightFromSquare = faUpRightFromSquare;
  faWrench = faWrench;
  faVolumeUp = faVolumeUp;

  // Modals
  createProxyCheckTargetModalVisible = false;
  updateProxyCheckTargetModalVisible = false;
  createCustomSnippetModalVisible = false;
  updateCustomSnippetModalVisible = false;
  changeAdminPasswordModalVisible = false;
  changeAdminApiKeyModalVisible = false;
  createRemoteConfigsEndpointModalVisible = false;
  updateRemoteConfigsEndpointModalVisible = false;
  addThemeModalVisible = false;

  fieldsValidity: { [key: string]: boolean } = {};
  settings: OBSettingsDto | null = null;
  themes: string[] | null = null;
  touched = false;
  configSections: ConfigSection[] = [
    ConfigSection.Metadata,
    ConfigSection.Readme,
    ConfigSection.Stacker,
    ConfigSection.LoliCode,
    ConfigSection.Settings,
    ConfigSection.CSharpCode,
    ConfigSection.LoliScript,
  ];
  jobDisplayModes: JobDisplayMode[] = [JobDisplayMode.Standard, JobDisplayMode.Detailed];
  selectedProxyCheckTarget: ProxyCheckTarget | null = null;
  selectedCustomSnippet: CustomSnippet | null = null;
  selectedRemoteConfigsEndpoint: RemoteConfigsEndpoint | null = null;

  constructor(
    private settingsService: SettingsService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
  ) { }

  canDeactivate() {
    if (!this.touched) {
      return true;
    }

    // Ask for confirmation and return the observable
    return new Observable<boolean>((observer) => {
      this.confirmationService.confirm({
        message: 'You have unsaved changes. Are you sure that you want to leave?',
        header: 'Confirmation',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          observer.next(true);
          observer.complete();
        },
        reject: () => {
          observer.next(false);
          observer.complete();
        },
      });
    });
  }

  ngOnInit(): void {
    this.getSettings();
    this.getThemes();
  }

  getSettings() {
    this.settingsService.getSettings().subscribe((settings) => {
      this.settings = settings;
    });
  }

  getThemes() {
    this.settingsService.getAllThemes().subscribe((themes) => {
      this.themes = ['None', ...themes.map((t) => t.name)];
    });
  }

  saveSettings() {
    if (this.settings === null) return;
    this.settingsService.updateSettings(this.settings).subscribe((settings) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Saved',
        detail: 'The settings were successfully saved',
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
      accept: () => this.restoreDefaults(),
    });
  }

  restoreDefaults() {
    this.settings = null;
    this.settingsService.getDefaultSettings().subscribe((defaultSettings) => {
      this.settingsService.updateSettings(defaultSettings).subscribe((settings) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Restored',
          detail: 'Settings restored to the default values',
        });
        this.settings = settings;
        applyAppTheme();
      });
    });
  }

  onValidityChange(validity: FieldValidity) {
    this.fieldsValidity = {
      ...this.fieldsValidity,
      [validity.key]: validity.valid,
    };
  }

  // Can save if touched and every field is valid
  canSave() {
    return this.touched && Object.values(this.fieldsValidity).every((v) => v);
  }

  openCreateProxyCheckTargetModal() {
    this.createProxyCheckTargetModalVisible = true;
  }

  openUpdateProxyCheckTargetModal(target: ProxyCheckTarget) {
    this.selectedProxyCheckTarget = target;
    this.updateProxyCheckTargetModalVisible = true;
  }

  openAddThemeModal() {
    this.addThemeModalVisible = true;
  }

  createProxyCheckTarget(target: ProxyCheckTarget) {
    this.settings!.generalSettings.proxyCheckTargets.push(target);
    this.touched = true;
    this.createProxyCheckTargetModalVisible = false;
  }

  updateProxyCheckTarget(target: ProxyCheckTarget) {
    if (this.selectedProxyCheckTarget === null) return;
    this.selectedProxyCheckTarget.url = target.url;
    this.selectedProxyCheckTarget.successKey = target.successKey;
    this.touched = true;
    this.updateProxyCheckTargetModalVisible = false;
  }

  deleteProxyCheckTarget(target: ProxyCheckTarget) {
    const index = this.settings!.generalSettings.proxyCheckTargets.indexOf(target);
    if (index !== -1) {
      this.settings!.generalSettings.proxyCheckTargets.splice(index, 1);
      this.touched = true;
    }
  }

  openCreateCustomSnippetModal() {
    this.createCustomSnippetModalVisible = true;
  }

  openUpdateCustomSnippetModal(snippet: CustomSnippet) {
    this.selectedCustomSnippet = snippet;
    this.updateCustomSnippetModalVisible = true;
  }

  createCustomSnippet(snippet: CustomSnippet) {
    this.settings!.generalSettings.customSnippets.push(snippet);
    this.touched = true;
    this.createCustomSnippetModalVisible = false;
  }

  updateCustomSnippet(snippet: CustomSnippet) {
    if (this.selectedCustomSnippet === null) return;
    this.selectedCustomSnippet.name = snippet.name;
    this.selectedCustomSnippet.description = snippet.description;
    this.selectedCustomSnippet.body = snippet.body;
    this.touched = true;
    this.updateCustomSnippetModalVisible = false;
  }

  deleteCustomSnippet(snippet: CustomSnippet) {
    const index = this.settings!.generalSettings.customSnippets.indexOf(snippet);
    if (index !== -1) {
      this.settings!.generalSettings.customSnippets.splice(index, 1);
      this.touched = true;
    }
  }

  changeRequireAdminLogin(enabled: boolean) {
    this.settings!.securitySettings.requireAdminLogin = enabled;

    if (!enabled) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: `You have disabled the requirement for admin login.
        If you do this while your instance is exposed to the public internet,
        anyone will be able to access your instance without authentication!`,
        key: 'br',
        life: 10000,
      });
    }
  }

  openChangeAdminPasswordModal() {
    this.changeAdminPasswordModalVisible = true;
  }

  openChangeAdminApiKeyModal() {
    this.changeAdminApiKeyModalVisible = true;
  }

  changeAdminPassword(password: string) {
    this.settingsService.updateAdminPassword(password).subscribe(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Changed',
        detail: 'The admin password was changed',
      });
      this.changeAdminPasswordModalVisible = false;
    });
  }

  changeAdminApiKey(apiKey: string) {
    // Maybe in the future we can use a different endpoint for this
    if (this.settings === null) return;
    this.settings.securitySettings.adminApiKey = apiKey;
    this.touched = true;
    this.changeAdminApiKeyModalVisible = false;
  }

  openCreateRemoteConfigsEndpointModal() {
    this.createRemoteConfigsEndpointModalVisible = true;
  }

  openUpdateRemoteConfigsEndpointModal(endpoint: RemoteConfigsEndpoint) {
    this.selectedRemoteConfigsEndpoint = endpoint;
    this.updateRemoteConfigsEndpointModalVisible = true;
  }

  createRemoteConfigsEndpoint(endpoint: RemoteConfigsEndpoint) {
    this.settings!.remoteSettings.configsEndpoints.push(endpoint);
    this.touched = true;
    this.createRemoteConfigsEndpointModalVisible = false;
  }

  updateRemoteConfigsEndpoint(endpoint: RemoteConfigsEndpoint) {
    if (this.selectedRemoteConfigsEndpoint === null) return;
    this.selectedRemoteConfigsEndpoint.url = endpoint.url;
    this.selectedRemoteConfigsEndpoint.apiKey = endpoint.apiKey;
    this.touched = true;
    this.updateRemoteConfigsEndpointModalVisible = false;
  }

  deleteRemoteConfigsEndpoint(endpoint: RemoteConfigsEndpoint) {
    const index = this.settings!.remoteSettings.configsEndpoints.indexOf(endpoint);
    if (index !== -1) {
      this.settings!.remoteSettings.configsEndpoints.splice(index, 1);
      this.touched = true;
    }
  }

  uploadTheme(file: File) {
    this.settingsService.uploadTheme(file).subscribe(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Added',
        detail: `Added new theme ${file.name}`,
      });
      this.addThemeModalVisible = false;
      this.getThemes();
    });
  }

  onThemeChange(theme: string) {
    applyAppTheme(theme);
  }

  playHitSound() {
    this.hitSoundPlayer.nativeElement.play();
  }
}
