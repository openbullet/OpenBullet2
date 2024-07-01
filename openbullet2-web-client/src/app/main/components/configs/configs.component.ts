import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import {
  faClone,
  faDownload,
  faFilterCircleXmark,
  faGears,
  faPen,
  faTriangleExclamation,
  faX,
} from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { saveFile } from 'src/app/shared/utils/files';
import { ConfigInfoDto, ConfigMode } from '../../dtos/config/config-info.dto';
import { ConfigDto } from '../../dtos/config/config.dto';
import { OBSettingsDto } from '../../dtos/settings/ob-settings.dto';
import { ConfigService } from '../../services/config.service';
import { SettingsService } from '../../services/settings.service';
import { VolatileSettingsService } from '../../services/volatile-settings.service';
import { UploadConfigsComponent } from './upload-configs/upload-configs.component';

@Component({
  selector: 'app-configs',
  templateUrl: './configs.component.html',
  styleUrls: ['./configs.component.scss'],
})
export class ConfigsComponent implements OnInit {
  configs: ConfigInfoDto[] | null = null;
  obSettings: OBSettingsDto | null = null;

  @ViewChild('uploadConfigsComponent')
  uploadConfigsComponent: UploadConfigsComponent | undefined = undefined;

  faPen = faPen;
  faClone = faClone;
  faDownload = faDownload;
  faX = faX;
  faFilterCircleXmark = faFilterCircleXmark;
  faTriangleExclamation = faTriangleExclamation;
  faGears = faGears;

  // biome-ignore lint/suspicious/noExplicitAny: Moment
  moment: any = moment;

  displayMode = 'grid';
  uploadConfigsModalVisible = false;

  configMenuItems: MenuItem[] = [
    {
      id: 'file',
      label: 'File',
      icon: 'pi pi-fw pi-file',
      items: [
        {
          id: 'create-new',
          label: 'Create new',
          icon: 'pi pi-fw pi-plus color-good',
          command: (e) => this.safeCreateConfig(),
        },
        {
          id: 'upload-files',
          label: 'Upload',
          icon: 'pi pi-fw pi-upload color-good',
          command: (e) => this.openUploadConfigsModal(),
        },
        {
          id: 'download-all',
          label: 'Download all',
          icon: 'pi pi-fw pi-download color-accent-light',
          command: (e) => this.downloadAllConfigs(),
        },
        {
          id: 'reload-from-disk',
          label: 'Reload all',
          icon: 'pi pi-fw pi-refresh',
          command: (e) => this.refreshConfigs(true),
        },
      ],
    },
    {
      id: 'view',
      label: 'View',
      icon: 'pi pi-fw pi-desktop',
      items: [
        {
          id: 'display-as-table',
          label: 'Display as table',
          icon: 'pi pi-fw pi-bars',
          command: (e) => this.changeDisplayMode('table'),
        },
        {
          id: 'display-as-grid',
          label: 'Display as grid',
          icon: 'pi pi-fw pi-th-large',
          command: (e) => this.changeDisplayMode('grid'),
        },
      ],
    },
  ];

  constructor(
    private configService: ConfigService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private volatileSettings: VolatileSettingsService,
    private settingsService: SettingsService,
    private router: Router,
  ) { }

  ngOnInit(): void {
    this.refreshConfigs(false);
    this.displayMode = this.volatileSettings.configsDisplayMode;

    this.settingsService.getSettings().subscribe((settings) => {
      this.obSettings = settings;
    });
  }

  refreshConfigs(reload: boolean) {
    this.configService.getAllConfigs(reload).subscribe((configs) => {
      this.configs = configs;

      if (reload) {
        this.messageService.add({
          severity: 'success',
          summary: 'Reloaded',
          detail: `${configs.length} configs reloaded from disk`,
        });
      }
    });
  }

  changeDisplayMode(mode: string) {
    this.displayMode = mode;
    this.volatileSettings.configsDisplayMode = mode;
  }

  openUploadConfigsModal() {
    this.uploadConfigsComponent?.reset();
    this.uploadConfigsModalVisible = true;
  }

  safeEditConfig(config: ConfigInfoDto) {
    if (config.isRemote) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Remote config',
        detail: 'This config is stored remotely and cannot be edited',
      });
      return;
    }

    if (this.configService.hasUnsavedChanges()) {
      this.confirmationService.confirm({
        message: 'You have unsaved changes, are you sure you want to discard them?',
        header: 'Unsaved changes',
        icon: 'pi pi-exclamation-triangle',
        accept: () => this.selectConfig(config),
      });
    } else {
      this.selectConfig(config);
    }
  }

  selectConfig(config: ConfigInfoDto) {
    this.configService.getConfig(config.id).subscribe((resp) => {
      this.configService.selectConfig(resp);
      this.messageService.add({
        severity: 'success',
        summary: 'Selected',
        detail: `Selected config ${resp.metadata.name}`,
      });
      if (config.dangerous) {
        this.messageService.add({
          key: 'br',
          severity: 'warn',
          summary: 'Dangerous',
          detail:
            'This config could be dangerous as it might contain plain C# code, DO NOT run it unless you trust the source!',
          life: 10000,
        });
      }
      this.router.navigate([this.getRouteBySettings(resp)]);
    });
  }

  getRouteBySettings(config: ConfigDto): string {
    if (this.obSettings === null) {
      return '/config/metadata';
    }

    switch (this.obSettings.generalSettings.configSectionOnLoad) {
      case 'metadata':
        return 'config/metadata';

      case 'readme':
        return 'config/readme';

      case 'stacker':
        switch (config.mode) {
          case ConfigMode.LoliCode:
          case ConfigMode.Stack:
            return 'config/stacker';

          case ConfigMode.CSharp:
            return 'config/csharp';

          case ConfigMode.Legacy:
            return 'config/loliscript';

          default:
            return 'config/metadata';
        }

      case 'loliCode':
      case 'loliScript':
        switch (config.mode) {
          case ConfigMode.LoliCode:
          case ConfigMode.Stack:
            return 'config/lolicode';

          case ConfigMode.CSharp:
            return 'config/csharp';

          case ConfigMode.Legacy:
            return 'config/loliscript';

          default:
            return 'config/metadata';
        }

      case 'settings':
        return 'config/settings';

      case 'cSharpCode':
        switch (config.mode) {
          case ConfigMode.LoliCode:
          case ConfigMode.Stack:
          case ConfigMode.CSharp:
            return 'config/csharp';

          case ConfigMode.Legacy:
            return 'config/loliscript';

          default:
            return 'config/metadata';
        }

      default:
        return 'config/metadata';
    }
  }

  safeCreateConfig() {
    if (this.configService.hasUnsavedChanges()) {
      this.confirmationService.confirm({
        message: 'You have unsaved changes, are you sure you want to discard them?',
        header: 'Unsaved changes',
        icon: 'pi pi-exclamation-triangle',
        accept: () => this.createConfig(),
      });
    } else {
      this.createConfig();
    }
  }

  createConfig() {
    this.configService.createConfig().subscribe((newConfig) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Created',
        detail: 'Created a new config',
      });
      this.configService.selectConfig(newConfig);
      this.router.navigate(['config/metadata']);
    });
  }

  safeCloneConfig(config: ConfigInfoDto, event: MouseEvent) {
    event.stopPropagation();

    if (this.configService.hasUnsavedChanges()) {
      this.confirmationService.confirm({
        message: 'You have unsaved changes, are you sure you want to discard them?',
        header: 'Unsaved changes',
        icon: 'pi pi-exclamation-triangle',
        accept: () => this.cloneConfig(config),
      });
    } else {
      this.cloneConfig(config);
    }
  }

  cloneConfig(config: ConfigInfoDto) {
    this.configService.cloneConfig(config.id).subscribe(_ => {
      this.messageService.add({
        severity: 'success',
        summary: 'Cloned',
        detail: `Created a clone of ${config.name}`,
      });
      this.refreshConfigs(false);
      // TODO: Immediately edit the new config, do not refresh
    });
  }

  uploadConfigs(files: File[]) {
    this.configService.uploadConfigs(files).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Uploaded',
        detail: `Uploaded ${resp.count} configs`,
      });
      this.uploadConfigsModalVisible = false;
      this.refreshConfigs(false);
    });
  }

  downloadConfig(config: ConfigInfoDto, event: MouseEvent) {
    event.stopPropagation();

    this.configService.downloadConfig(config.id).subscribe((resp) => saveFile(resp));
  }

  downloadAllConfigs() {
    this.configService.downloadAllConfigs().subscribe((resp) => saveFile(resp));
  }

  confirmDeleteConfig(config: ConfigInfoDto, event: MouseEvent) {
    event.stopPropagation();

    this.confirmationService.confirm({
      message: `Are you really sure you want to delete the config ${config.name}?
      This is an irreversible operation, you will not be able to recover the config.`,
      header: 'Are you sure?',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteConfig(config),
    });
  }

  deleteConfig(config: ConfigInfoDto) {
    this.configService.deleteConfig(config.id).subscribe(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `Config ${config.name} was deleted`,
      });
      this.refreshConfigs(false);
    });
  }
}
