import { Component, HostListener, OnInit } from '@angular/core';
import { SettingsService } from '../../services/settings.service';
import { CustomSnippet, OBSettingsDto, ProxyCheckTarget, RemoteConfigsEndpoint } from '../../dtos/settings/ob-settings.dto';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { faLink, faPen, faPlus, faUpRightFromSquare, faWrench, faX } from '@fortawesome/free-solid-svg-icons';
import { ThemeDto } from '../../dtos/settings/theme.dto';
import { applyAppTheme } from 'src/app/shared/utils/theme';

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

  faPlus = faPlus;
  faX = faX;
  faPen = faPen;
  faLink = faLink;
  faUpRightFromSquare = faUpRightFromSquare;
  faWrench = faWrench;
  
  // Modals
  createProxyCheckTargetModalVisible = false;
  updateProxyCheckTargetModalVisible = false;
  createCustomSnippetModalVisible = false;
  updateCustomSnippetModalVisible = false;
  changeAdminPasswordModalVisible = false;
  createRemoteConfigsEndpointModalVisible = false;
  updateRemoteConfigsEndpointModalVisible = false;
  addThemeModalVisible = false;

  fieldsValidity: { [key: string] : boolean; } = {};
  settings: OBSettingsDto | null = null;
  themes: string[] | null = null;
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
  jobDisplayModes: string[] = [
    'standard',
    'detailed'
  ];
  selectedProxyCheckTarget: ProxyCheckTarget | null = null;
  selectedCustomSnippet: CustomSnippet | null = null;
  selectedRemoteConfigsEndpoint: RemoteConfigsEndpoint | null = null;

  constructor(private settingsService: SettingsService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService) {}
  
  ngOnInit(): void {
    this.getSettings();
    this.getThemes();
  }

  getSettings() {
    this.settingsService.getSettings()
      .subscribe(settings => this.settings = settings);
  }

  getThemes() {
    this.settingsService.getAllThemes()
      .subscribe(themes => {
        this.themes = [
          'None',
          ...themes.map(t => t.name)
        ];
      });
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
            applyAppTheme();
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

  openChangeAdminPasswordModal() {
    this.changeAdminPasswordModalVisible = true;
  }

  changeAdminPassword(password: string) {
    this.settingsService.updateAdminPassword(password)
      .subscribe(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Changed',
          detail: 'The admin password was changed'
        });
        this.changeAdminPasswordModalVisible = false;
      });
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
    this.settingsService.uploadTheme(file)
      .subscribe(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Added',
          detail: `Added new theme ${file.name}`
        });
        this.addThemeModalVisible = false;
        this.getThemes();
      });
  }

  onThemeChange(theme: string) {
    applyAppTheme(theme);
  }
}
