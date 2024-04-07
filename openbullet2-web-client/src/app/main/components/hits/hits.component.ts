import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../../services/settings.service';
import { HitService } from '../../services/hit.service';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { PagedList } from '../../dtos/common/paged-list.dto';
import { HitDto } from '../../dtos/hit/hit.dto';
import { EnvironmentSettingsDto } from '../../dtos/settings/environment-settings.dto';
import * as moment from 'moment';
import { saveFile } from 'src/app/shared/utils/files';
import { faDatabase, faFilterCircleXmark, faPen, faX } from '@fortawesome/free-solid-svg-icons';
import { UpdateHitDto } from '../../dtos/hit/update-hit.dto';
import { UserService } from '../../services/user.service';
import { HitSortField } from '../../dtos/hit/hit-filters.dto';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-hits',
  templateUrl: './hits.component.html',
  styleUrls: ['./hits.component.scss'],
})
export class HitsComponent implements OnInit {
  envSettings: EnvironmentSettingsDto | null = null;
  hits: PagedList<HitDto> | null = null;
  rowCount: number = 10;

  faPen = faPen;
  faX = faX;
  faDatabase = faDatabase;
  faFilterCircleXmark = faFilterCircleXmark;

  searchTerm: string = '';
  hitType: string = 'Any Type';
  hitTypes: string[] = ['Any Type', 'SUCCESS', 'NONE'];

  configName: string = 'anyConfig';
  configNames: string[] = ['anyConfig'];

  sortBy: HitSortField = HitSortField.Date;
  sortDescending: boolean = true;

  rangeDates: Date[] = [moment().subtract(7, 'days').toDate(), moment().endOf('day').toDate()];

  selectedHit: HitDto | null = null;
  updateHitModalVisible = false;

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
              items: [], // This array is filled at runtime
            },
            {
              id: 'send-filtered-hits-to-recheck',
              label: 'Send to recheck',
              icon: 'pi pi-fw pi-arrow-right color-accent-light',
              command: (e) => console.log('SEND TO RECHECK!'), // TODO: Implement
            },
            {
              id: 'delete-filtered-hits',
              label: 'Delete',
              icon: 'pi pi-fw pi-trash color-bad',
              command: (e) => this.deleteFilteredHits(),
            },
          ],
        },
      ],
    },
    {
      id: 'delete',
      label: 'Delete',
      icon: 'pi pi-fw pi-trash',
      items: [
        {
          id: 'purge-hits',
          label: 'Purge all hits',
          icon: 'pi pi-fw pi-trash color-bad',
          visible: this.userService.isAdmin(),
          command: (e) => this.confirmPurgeHits(),
        },
        {
          id: 'delete-duplicate-hits',
          label: 'Duplicates',
          icon: 'pi pi-fw pi-trash color-bad',
          command: (e) => this.deleteDuplicateHits(),
        },
      ],
    },
  ];

  constructor(
    private settingsService: SettingsService,
    private hitService: HitService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private userService: UserService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.activatedRoute.queryParams.subscribe((params) => {
      this.searchTerm = params['searchTerm'] ?? '';
      this.hitType = params['hitType'] ?? 'Any Type';
      this.configName = params['configName'] ?? 'anyConfig';
      this.rangeDates = [
        moment(params['minDate'] ?? moment().subtract(7, 'days').toISOString()).toDate(),
        moment(params['maxDate'] ?? moment().endOf('day').toISOString()).toDate(),
      ];
      this.sortBy = params['sortBy'] ?? HitSortField.Date;
      this.sortDescending = params['sortDescending'] === 'true';

      this.settingsService.getEnvironmentSettings().subscribe((envSettings) => {
        this.envSettings = envSettings;
        this.hitTypes = ['Any Type', 'SUCCESS', 'NONE', ...envSettings.customStatuses.map((cs) => cs.name)];

        // Update the "Export with format" menu items
        const exportMenuItems = envSettings.exportFormats.map((ef) => {
          return {
            label: ef.format,
            command: () => this.downloadHits(ef.format),
            styleClass: 'long-menu-item',
          };
        });

        this.hitMenuItems
          .find((i) => i.id === 'edit')!
          .items!.find((i) => i.id === 'edit-filtered-hits')!
          .items!.find((i) => i.id === 'export-filtered-hits')!.items = exportMenuItems;

        this.refreshHits();
      });

      this.hitService.getConfigNames().subscribe((configNames) => {
        this.configNames = ['anyConfig', ...configNames];
      });
    });
  }

  // TODO: Only call this when necessary, don't make double calls!
  refreshHits(
    pageNumber: number = 1,
    pageSize: number | null = null,
    sortBy: HitSortField = HitSortField.Date,
    sortDescending: boolean = true,
  ) {
    if (this.envSettings === null) return;

    // Update the URL with the new filters WITHOUT reloading the page
    // Do not add stuff to the URL if it's the default value
    window.history.replaceState(
      {},
      '',
      this.router
        .createUrlTree([], {
          relativeTo: this.activatedRoute,
          queryParams: {
            searchTerm: this.searchTerm === '' ? null : this.searchTerm,
            hitType: this.hitType === 'Any Type' ? null : this.hitType,
            configName: this.configName === 'anyConfig' ? null : this.configName,
            minDate: moment(this.rangeDates[0]).toISOString(),
            maxDate: moment(this.rangeDates[1]).toISOString(),
            sortBy: sortBy === HitSortField.Date ? null : sortBy,
            sortDescending: sortDescending ? 'true' : null,
          },
        })
        .toString(),
    );

    this.hitService
      .getHits({
        pageNumber,
        pageSize: pageSize ?? this.rowCount,
        searchTerm: this.searchTerm,
        type: this.hitType === 'Any Type' ? null : this.hitType,
        configName: this.configName === 'anyConfig' ? null : this.configName,
        minDate: this.rangeDates[0].toISOString(),
        maxDate: this.rangeDates[1].toISOString(),
        sortBy: sortBy,
        sortDescending: sortDescending,
      })
      .subscribe((hits) => (this.hits = hits));
  }

  lazyLoadHits(event: any) {
    this.refreshHits(
      Math.floor(event.first / event.rows) + 1,
      event.rows,
      event.sortField as HitSortField,
      event.sortOrder === -1,
    );
  }

  clearFilters() {
    this.searchTerm = '';
    this.hitType = 'Any Type';
    this.configName = 'anyConfig';
    this.rangeDates = [moment().subtract(7, 'days').toDate(), moment().endOf('day').toDate()];

    this.refreshHits();
  }

  searchBoxKeyDown(event: any) {
    if (event.key == 'Enter') {
      this.refreshHits();
    }
  }

  deleteFilteredHits() {
    this.hitService
      .deleteHits({
        pageNumber: null,
        pageSize: null,
        searchTerm: this.searchTerm,
        type: this.hitType === 'Any Type' ? null : this.hitType,
        minDate: this.rangeDates[0].toISOString(),
        maxDate: this.rangeDates[1].toISOString(),
        configName: this.configName === 'anyConfig' ? null : this.configName,
        sortBy: null,
        sortDescending: false,
      })
      .subscribe((resp) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Deleted',
          detail: `${resp.count} hits were deleted from the database`,
        });
        this.refreshHits();
      });
  }

  deleteDuplicateHits() {
    this.hitService.deleteDuplicateHits().subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `${resp.count} hits were deleted from the database`,
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
      accept: () => this.purgeHits(),
    });
  }

  purgeHits() {
    this.hitService.purgeHits().subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `All ${resp.count} hits were deleted from the database`,
      });
      this.refreshHits();
    });
  }

  downloadHits(format: string) {
    this.hitService
      .downloadHits(
        {
          pageNumber: null,
          pageSize: null,
          searchTerm: this.searchTerm,
          type: this.hitType === 'Any Type' ? null : this.hitType,
          minDate: this.rangeDates[0].toISOString(),
          maxDate: this.rangeDates[1].toISOString(),
          configName: this.configName === 'anyConfig' ? null : this.configName,
          sortBy: null,
          sortDescending: false,
        },
        format,
      )
      .subscribe((resp) => saveFile(resp));
  }

  getTypeColor(type: string) {
    if (type === 'SUCCESS') {
      return 'yellowgreen';
    } else if (type === 'NONE') {
      return '#7FFFD4';
    }

    // Check if there is a custom status
    if (this.envSettings !== null) {
      const customStatus = this.envSettings.customStatuses.find((cs) => cs.name === type);

      if (customStatus !== undefined) {
        return customStatus.color;
      }
    }

    return 'darkorange';
  }

  openUpdateHitModal(hit: HitDto) {
    this.selectedHit = hit;
    this.updateHitModalVisible = true;
  }

  confirmDeleteHit(hit: HitDto) {
    this.confirmationService.confirm({
      message: `You are about to delete the hit ${hit.data}. 
      Are you sure that you want to proceed?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteHit(hit),
    });
  }

  deleteHit(hit: HitDto) {
    this.hitService.deleteHit(hit.id).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `Hit ${hit.data} was deleted`,
      });
      this.refreshHits();
    });
  }

  updateHit(updated: UpdateHitDto) {
    this.hitService.updateHit(updated).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Updated',
        detail: `Hit ${resp.data} was updated`,
      });
      this.updateHitModalVisible = false;
      this.refreshHits();
    });
  }
}
