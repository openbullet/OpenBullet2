import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../../services/settings.service';
import { HitService } from '../../services/hit.service';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { PagedList } from '../../dtos/common/paged-list.dto';
import { HitDto } from '../../dtos/hit/hit.dto';
import { EnvironmentSettingsDto } from '../../dtos/settings/environment-settings.dto';
import * as moment from 'moment';
import { saveFile } from 'src/app/shared/utils/files';

@Component({
  selector: 'app-hits',
  templateUrl: './hits.component.html',
  styleUrls: ['./hits.component.scss']
})
export class HitsComponent implements OnInit {
  envSettings: EnvironmentSettingsDto | null = null;
  hits: PagedList<HitDto> | null = null;
  rowCount: number = 10;

  searchTerm: string = '';
  hitType: string = 'Any Type';
  hitTypes: string[] = [
    'Any Type',
    'SUCCESS',
    'NONE'
  ];

  // TODO: Add calendar to UI
  minDate: Date = moment().subtract(7, 'days').toDate();
  maxDate: Date = moment().endOf('day').toDate();

  hitMenuItems: MenuItem[] = [
    {
      id: 'edit',
      label: 'Edit',
      icon: 'pi pi-fw pi-pencil',
      items: [
        {
          id: 'edit-filtered-hits',
          label: 'Filtered hits',
          icon: 'pi pi-fw pi-filter-fill',
          items: [
            {
              id: 'export-filtered-hits',
              label: 'Export with format',
              icon: 'pi pi-fw pi-file-export color-accent-light',
              items: [] // This array is filled at runtime
            },
            {
              id: 'send-filtered-hits-to-recheck',
              label: 'Send to recheck',
              icon: 'pi pi-fw pi-arrow-right color-accent-light',
              command: e => console.log('SEND TO RECHECK!')
            },
            {
              id: 'delete-filtered-hits',
              label: 'Delete',
              icon: 'pi pi-fw pi-trash color-bad',
              command: e => this.deleteFilteredHits()
            }
          ]
        }
      ]
    },
    {
      id: 'delete',
      label: 'Delete',
      icon: 'pi pi-fw pi-trash',
      items: [
        {
          // TODO: This is only displayed if the user is admin
          id: 'purge-hits',
          label: 'Purge all hits',
          icon: 'pi pi-fw pi-trash color-bad',
          command: e => this.confirmPurgeHits()
        },
        {
          id: 'delete-duplicate-hits',
          label: 'Duplicates',
          icon: 'pi pi-fw pi-trash color-bad',
          command: e => this.deleteDuplicateHits()
        }
      ]
    }
  ];

  constructor(private settingsService: SettingsService,
    private hitService: HitService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService) {

    }

  ngOnInit(): void {
    this.settingsService.getEnvironmentSettings()
      .subscribe(envSettings => {
        this.envSettings = envSettings;
        this.hitTypes = [
          'Any Type',
          'SUCCESS',
          'NONE',
          ...envSettings.customStatuses.map(cs => cs.name)
        ];

        // Update the "Export with format" menu items
        const exportMenuItems = envSettings.exportFormats.map(ef => {
          return {
            label: ef.format,
            command: () => this.downloadHits(ef.format),
            styleClass: 'long-menu-item'
          }
        });

        this.hitMenuItems
          .find(i => i.id === 'edit')!
          .items!.find(i => i.id === 'edit-filtered-hits')!
          .items!.find(i => i.id === 'export-filtered-hits')!
          .items = exportMenuItems;

        this.refreshHits();
      });
  }

  // TODO: Only call this when necessary, don't make double calls!
  refreshHits(pageNumber: number = 1, pageSize: number | null = null) {
    if (this.envSettings === null) return;

    this.hitService.getHits({
      pageNumber,
      pageSize: pageSize ?? this.rowCount,
      searchTerm: this.searchTerm,
      hitType: this.hitType === 'Any Type' ? null : this.hitType,
      minDate: this.minDate.toISOString(),
      maxDate: this.maxDate.toISOString()
    }).subscribe(hits => this.hits = hits);
  }

  lazyLoadHits(event: any) {
    this.refreshHits(
      Math.floor(event.first / event.rows) + 1,
      event.rows
    );
  }

  searchBoxKeyDown(event: any) {
    if (event.key == 'Enter') {
      this.refreshHits();
    }
  }

  deleteFilteredHits() {
    this.hitService.deleteHits({
      pageNumber: null,
      pageSize: null,
      searchTerm: this.searchTerm,
      hitType: this.hitType === 'Any Type' ? null : this.hitType,
      minDate: this.minDate.toISOString(),
      maxDate: this.maxDate.toISOString()
    }).subscribe(resp => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `${resp.count} hits were deleted from the database`
      });
      this.refreshHits();
    });
  }

  deleteDuplicateHits() {
    this.hitService.deleteDuplicateHits()
    .subscribe(resp => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `${resp.count} hits were deleted from the database`
      });
      this.refreshHits();
    });
  }

  confirmPurgeHits() {
    this.confirmationService.confirm({
      message: `You are about to purge all hits from the database,
      regardless of the filters that you set. You will have zero hits after
      this operation. Are you sure that you want to proceed?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.purgeHits()
    });
  }

  purgeHits() {
    this.hitService.purgeHits()
    .subscribe(resp => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `All ${resp.count} hits were deleted from the database`
      });
      this.refreshHits();
    });
  }

  downloadHits(format: string) {
    this.hitService.downloadHits({
      pageNumber: null,
      pageSize: null,
      searchTerm: this.searchTerm,
      hitType: this.hitType === 'Any Type' ? null : this.hitType,
      minDate: this.minDate.toISOString(),
      maxDate: this.maxDate.toISOString()
    }, format).subscribe(resp => saveFile(resp));
  }
}
