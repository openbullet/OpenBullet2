import { Component, OnInit, ViewChild } from '@angular/core';
import { ConfigInfoDto } from '../../dtos/config/config-info.dto';
import { faClone, faDownload, faFilterCircleXmark, faPen, faTriangleExclamation, faX } from '@fortawesome/free-solid-svg-icons';
import { ConfigService } from '../../services/config.service';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { saveFile } from 'src/app/shared/utils/files';
import { UploadConfigsComponent } from './upload-configs/upload-configs.component';
import * as moment from 'moment';

@Component({
  selector: 'app-configs',
  templateUrl: './configs.component.html',
  styleUrls: ['./configs.component.scss']
})
export class ConfigsComponent implements OnInit {
  configs: ConfigInfoDto[] | null = null;

  @ViewChild('uploadConfigsComponent')
  uploadConfigsComponent: UploadConfigsComponent | undefined = undefined;

  faPen = faPen;
  faClone = faClone;
  faDownload = faDownload;
  faX = faX;
  faFilterCircleXmark = faFilterCircleXmark;
  faTriangleExclamation = faTriangleExclamation;

  moment: any = moment;

  displayAsTable: boolean = true;
  uploadConfigsModalVisible: boolean = false;

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
          command: e => this.createConfig()
        },
        {
          id: 'upload-files',
          label: 'Upload',
          icon: 'pi pi-fw pi-upload color-good',
          command: e => this.openUploadConfigsModal()
        },
        {
          id: 'download-all',
          label: 'Download all',
          icon: 'pi pi-fw pi-download color-accent-light',
          command: e => this.downloadAllConfigs()
        },
        {
          id: 'reload-from-disk',
          label: 'Reload from disk',
          icon: 'pi pi-fw pi-refresh',
          command: e => this.refreshConfigs(true)
        }
      ]
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
          command: e => this.displayAsTable = true
        },
        {
          id: 'display-as-grid',
          label: 'Display as grid',
          icon: 'pi pi-fw pi-th-large',
          command: e => this.displayAsTable = false
        }
      ]
    }
  ];

  constructor(private configService: ConfigService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService) {

  }

  ngOnInit(): void {
    this.refreshConfigs(false);
  }

  refreshConfigs(reload: boolean) {
    this.configService.getAllConfigs(reload)
      .subscribe(configs => {
        this.configs = configs;

        if (reload) {
          this.messageService.add({
            severity: 'success',
            summary: 'Reloaded',
            detail: `${configs.length} configs reloaded from disk`
          });
        }
      });
  }

  openUploadConfigsModal() {
    this.uploadConfigsComponent?.reset();
    this.uploadConfigsModalVisible = true;
  }

  editConfig(config: ConfigInfoDto) {
    console.log('Edit config');
    // TODO: Implement
  }

  createConfig() {
    this.configService.createConfig()
      .subscribe(newConfig => {
        this.messageService.add({
          severity: 'success',
          summary: 'Created',
          detail: 'Created a new config'
        });
        this.refreshConfigs(false);
        // TODO: Immediately edit the new config, do not refresh
      });
  }

  cloneConfig(config: ConfigInfoDto) {
    this.configService.cloneConfig(config.id)
      .subscribe(newConfig => {
        this.messageService.add({
          severity: 'success',
          summary: 'Cloned',
          detail: `Created a clone of ${config.name}`
        });
        this.refreshConfigs(false);
        // TODO: Immediately edit the new config, do not refresh
      })
  }

  uploadConfigs(files: File[]) {
    this.configService.uploadConfigs(files)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Uploaded',
          detail: `Uploaded ${resp.count} configs`
        });
        this.uploadConfigsModalVisible = false;
        this.refreshConfigs(false);
      });
  }

  downloadConfig(config: ConfigInfoDto) {
    this.configService.downloadConfig(config.id)
      .subscribe(resp => saveFile(resp));
  }

  downloadAllConfigs() {
    this.configService.downloadAllConfigs()
      .subscribe(resp => saveFile(resp));
  }

  confirmDeleteConfig(config: ConfigInfoDto) {
    this.confirmationService.confirm({
      message: `Are you really sure you want to delete the config ${config.name}?
      This is an irreversible operation, you will not be able to recover the config.`,
      header: 'Are you sure?',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteConfig(config)
    });
  }

  deleteConfig(config: ConfigInfoDto) {
    this.configService.deleteConfig(config.id)
      .subscribe(() => {
        this.messageService.add({
          severity: 'success',
          summary: 'Deleted',
          detail: `Config ${config.name} was deleted`
        });
        this.refreshConfigs(false);
      });
  }
}
