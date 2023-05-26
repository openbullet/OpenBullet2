import { Component, HostListener, OnInit } from '@angular/core';
import { SettingsService } from '../../services/settings.service';
import { CustomSnippet, OBSettingsDto, ProxyCheckTarget } from '../../dtos/settings/ob-settings.dto';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { faLink, faPen, faPlus, faUpRightFromSquare, faX } from '@fortawesome/free-solid-svg-icons';

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
  
  // Modals
  createProxyCheckTargetModalVisible = false;
  updateProxyCheckTargetModalVisible = false;
  createCustomSnippetModalVisible = false;
  updateCustomSnippetModalVisible = false;
  changeAdminPasswordModalVisible = false;

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
  jobDisplayModes: string[] = [
    'standard',
    'detailed'
  ];
  selectedProxyCheckTarget: ProxyCheckTarget | null = null;
  selectedCustomSnippet: CustomSnippet | null = null;

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

  openCreateProxyCheckTargetModal() {
    this.createProxyCheckTargetModalVisible = true;
  }

  openUpdateProxyCheckTargetModal(target: ProxyCheckTarget) {
    this.selectedProxyCheckTarget = target;
    this.updateProxyCheckTargetModalVisible = true;
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
      });
  }

  showSuccess() {
    this.messageService.add({
      severity: 'success',
      summary: 'Test',
      detail: 'This is a test message',
      sticky: true
    });
  }
  
  showInfo() {
    this.messageService.add({
      severity: 'info',
      summary: 'Test',
      detail: 'This is a test message',
      sticky: true
    });
  }

  showWarn() {
    this.messageService.add({
      severity: 'warn',
      summary: 'Test',
      detail: 'This is a test message',
      sticky: true
    });
  }

  showError() {
    this.messageService.add({
      severity: 'error',
      summary: 'Test',
      detail: 'This is a test message',
      sticky: true
    });
  }
}
