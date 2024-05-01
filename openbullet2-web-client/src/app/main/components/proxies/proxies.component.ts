import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  faEye,
  faEyeSlash,
  faFileShield,
  faFilterCircleXmark,
  faKey,
  faPen,
  faPlus,
  faX,
} from '@fortawesome/free-solid-svg-icons';
import { ConfirmationService, MessageService } from 'primeng/api';
import { MenuItem } from 'primeng/api';
import { saveFile } from 'src/app/shared/utils/files';
import { PagedList } from '../../dtos/common/paged-list.dto';
import { CreateProxyGroupDto } from '../../dtos/proxy-group/create-proxy-group.dto';
import { ProxyGroupDto } from '../../dtos/proxy-group/proxy-group.dto';
import { UpdateProxyGroupDto } from '../../dtos/proxy-group/update-proxy-group.dto';
import { ProxyFiltersDto, ProxySortField } from '../../dtos/proxy/proxy-filters.dto';
import { ProxyDto } from '../../dtos/proxy/proxy.dto';
import { ProxyType } from '../../enums/proxy-type';
import { ProxyWorkingStatus } from '../../enums/proxy-working-status';
import { ProxyGroupService } from '../../services/proxy-group.service';
import { ProxyService } from '../../services/proxy.service';
import { CreateProxyGroupComponent } from './create-proxy-group/create-proxy-group.component';
import { DeleteSlowProxiesParams } from './delete-slow-proxies/delete-slow-proxies.component';
import { ImportProxiesFromFileComponent } from './import-proxies-from-file/import-proxies-from-file.component';
import {
  ImportProxiesFromRemoteComponent,
  RemoteProxiesToImport,
} from './import-proxies-from-remote/import-proxies-from-remote.component';
import {
  ImportProxiesFromTextComponent,
  ProxiesToImport,
} from './import-proxies-from-text/import-proxies-from-text.component';

@Component({
  selector: 'app-proxies',
  templateUrl: './proxies.component.html',
  styleUrls: ['./proxies.component.scss'],
})
export class ProxiesComponent implements OnInit {
  @ViewChild('createProxyGroupComponent')
  createProxyGroupComponent: CreateProxyGroupComponent | undefined;

  @ViewChild('importProxiesFromTextComponent')
  importProxiesFromTextComponent: ImportProxiesFromTextComponent | undefined;

  @ViewChild('importProxiesFromFileComponent')
  importProxiesFromFileComponent: ImportProxiesFromFileComponent | undefined;

  @ViewChild('importProxiesFromRemoteComponent')
  importProxiesFromRemoteComponent: ImportProxiesFromRemoteComponent | undefined;

  proxyGroups: ProxyGroupDto[] | null = null;
  proxies: PagedList<ProxyDto> | null = null;
  rowCount = 10;

  faPen = faPen;
  faKey = faKey;
  faFilterCircleXmark = faFilterCircleXmark;
  faX = faX;
  faPlus = faPlus;
  faEye = faEye;
  faEyeSlash = faEyeSlash;
  faFileShield = faFileShield;

  defaultProxyGroup = {
    id: -1,
    name: 'All',
    owner: { id: -2, username: 'System' },
  };

  selectedProxyGroup: ProxyGroupDto = this.defaultProxyGroup;
  showPasswords = false;

  createProxyGroupModalVisible = false;
  updateProxyGroupModalVisible = false;
  deleteSlowProxiesModalVisible = false;
  importProxiesFromTextModalVisible = false;
  importProxiesFromFileModalVisible = false;
  importProxiesFromRemoteModalVisible = false;

  searchTerm = '';
  proxyWorkingStatus: 'anyStatus' | ProxyWorkingStatus = 'anyStatus';
  proxyWorkingStatuses: ('anyStatus' | ProxyWorkingStatus)[] = [
    'anyStatus',
    ProxyWorkingStatus.Untested,
    ProxyWorkingStatus.Working,
    ProxyWorkingStatus.NotWorking,
  ];
  proxyType: 'anyType' | ProxyType = 'anyType';
  proxyTypes: ('anyType' | ProxyType)[] = [
    'anyType',
    ProxyType.Http,
    ProxyType.Socks4,
    ProxyType.Socks4a,
    ProxyType.Socks5,
  ];
  sortBy: ProxySortField = ProxySortField.LastChecked;
  sortDescending = true;

  proxyMenuItems: MenuItem[] = [
    {
      id: 'import',
      label: 'Import',
      icon: 'pi pi-fw pi-file-import',
      items: [
        {
          id: 'import-from-text',
          label: 'From text',
          icon: 'pi pi-fw pi-bars color-good',
          command: (e) => this.openImportProxiesFromTextModal(),
        },
        {
          id: 'import-from-file',
          label: 'From file',
          icon: 'pi pi-fw pi-file color-good',
          command: (e) => this.openImportProxiesFromFileModal(),
        },
        {
          id: 'import-from-remote',
          label: 'From remote',
          icon: 'pi pi-fw pi-globe color-good',
          command: (e) => this.openImportProxiesFromRemoteModal(),
        },
      ],
    },
    {
      id: 'edit',
      label: 'Edit',
      icon: 'pi pi-fw pi-pencil',
      items: [
        {
          id: 'edit-filtered-proxies',
          label: 'Filtered proxies',
          icon: 'pi pi-fw pi-filter-fill',
          items: [
            {
              id: 'export-filtered-proxies',
              label: 'Export',
              icon: 'pi pi-fw pi-file-export color-accent-light',
              command: (e) => this.downloadProxies(),
            },
            {
              id: 'move-filtered-proxies',
              label: 'Move to',
              icon: 'pi pi-fw pi-arrow-right color-accent-light',
              visible: false,
            },
            {
              id: 'delete-filtered-proxies',
              label: 'Delete',
              icon: 'pi pi-fw pi-trash color-bad',
              command: (e) => this.deleteFilteredProxies(),
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
          id: 'delete-all-proxies',
          label: 'All proxies',
          icon: 'pi pi-fw pi-trash color-bad',
          command: (e) => this.confirmDeleteAllProxies(),
        },
        {
          id: 'delete-not-working-proxies',
          label: 'Not working proxies',
          icon: 'pi pi-fw pi-trash color-bad',
          command: (e) => this.deleteNotWorkingProxies(),
        },
        {
          id: 'delete-untested-proxies',
          label: 'Untested proxies',
          icon: 'pi pi-fw pi-trash color-bad',
          command: (e) => this.deleteUntestedProxies(),
        },
        {
          id: 'delete-slow-proxies',
          label: 'Slow proxies',
          icon: 'pi pi-fw pi-trash color-bad',
          command: (e) => this.openDeleteSlowProxiesModal(),
        },
      ],
    },
  ];

  constructor(
    private proxyGroupService: ProxyGroupService,
    private proxyService: ProxyService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
  ) { }

  ngOnInit(): void {
    this.activatedRoute.queryParams.subscribe((params) => {
      this.searchTerm = params['searchTerm'] ?? '';
      this.proxyWorkingStatus = params['proxyWorkingStatus'] ?? 'anyStatus';
      this.proxyType = params['proxyType'] ?? 'anyType';
      this.sortBy = params['sortBy'];
      this.sortDescending = params['sortDescending'] === 'true';
      this.refreshProxyGroups();
    });
  }

  refreshProxyGroups(preselectProxyGroupId: number | null = null) {
    this.proxyGroupService.getAllProxyGroups().subscribe((proxyGroups) => {
      this.proxies = null;

      if (preselectProxyGroupId) {
        const preselectedProxyGroup = proxyGroups.find((pg) => pg.id === preselectProxyGroupId);
        if (preselectedProxyGroup) {
          this.selectedProxyGroup = preselectedProxyGroup;
        }
      } else {
        this.selectedProxyGroup = this.defaultProxyGroup;
      }

      this.proxyGroups = [this.defaultProxyGroup, ...proxyGroups];
      this.refreshProxies();

      // Update the "Move to" menu items
      const moveToMenuItems = proxyGroups.map((pg) => {
        return {
          label: pg.name,
          command: () => this.moveProxies(pg),
        };
      });

      const moveToMenuItem = this.proxyMenuItems
        .find((i) => i.id === 'edit')!
        .items!.find((i) => i.id === 'edit-filtered-proxies')!
        .items!.find((i) => i.id === 'move-filtered-proxies')!;

      if (moveToMenuItems.length > 0) {
        moveToMenuItem.items = moveToMenuItems;
        moveToMenuItem.visible = true;
      } else {
        moveToMenuItem.visible = false;
      }
    });
  }

  // TODO: Only call this when necessary, don't make double calls!
  refreshProxies(
    pageNumber = 1,
    pageSize: number | null = null,
    sortBy: ProxySortField = ProxySortField.LastChecked,
    sortDescending = true,
  ) {
    // Update the URL with the new filters WITHOUT reloading the page
    // Do not add stuff to the URL if it's the default value
    window.history.replaceState(
      {},
      '',
      this.router
        .createUrlTree([], {
          relativeTo: this.activatedRoute,
          queryParams: {
            searchTerm: this.searchTerm || null,
            proxyWorkingStatus: this.proxyWorkingStatus === 'anyStatus' ? null : this.proxyWorkingStatus,
            proxyType: this.proxyType === 'anyType' ? null : this.proxyType,
            sortBy: sortBy,
            sortDescending: sortDescending ? 'true' : null,
          },
        })
        .toString(),
    );
    this.proxyService
      .getProxies({
        pageNumber,
        pageSize: pageSize ?? this.rowCount,
        proxyGroupId: this.selectedProxyGroup.id,
        searchTerm: this.searchTerm,
        type: this.proxyType === 'anyType' ? null : this.proxyType,
        status: this.proxyWorkingStatus === 'anyStatus' ? null : this.proxyWorkingStatus,
        sortBy: sortBy,
        sortDescending: sortDescending,
      })
      .subscribe((proxies) => {
        this.proxies = proxies;
      });
  }

  // biome-ignore lint/suspicious/noExplicitAny: Event
  lazyLoadProxies(event: any) {
    this.refreshProxies(
      Math.floor(event.first / event.rows) + 1,
      event.rows,
      event.sortField as ProxySortField,
      event.sortOrder === -1,
    );
  }

  clearFilters() {
    this.searchTerm = '';
    this.proxyWorkingStatus = 'anyStatus';
    this.proxyType = 'anyType';
    this.refreshProxies();
  }

  proxyGroupSelected() {
    this.refreshProxies();
  }

  openCreateProxyGroupModal() {
    this.createProxyGroupComponent?.reset();
    this.createProxyGroupModalVisible = true;
  }

  openUpdateProxyGroupModal() {
    this.updateProxyGroupModalVisible = true;
  }

  openImportProxiesFromTextModal() {
    // Cannot import without choosing a group first
    if (this.selectedProxyGroup.id === -1) {
      this.messageService.add({
        severity: 'error',
        summary: 'No proxy group selected',
        detail: 'Select a valid proxy group, or create one, before importing proxies',
      });
      return;
    }

    this.importProxiesFromTextComponent?.reset();
    this.importProxiesFromTextModalVisible = true;
  }

  openImportProxiesFromFileModal() {
    // Cannot import without choosing a group first
    if (this.selectedProxyGroup.id === -1) {
      this.messageService.add({
        severity: 'error',
        summary: 'No proxy group selected',
        detail: 'Select a valid proxy group, or create one, before importing proxies',
      });
      return;
    }

    this.importProxiesFromFileComponent?.reset();
    this.importProxiesFromFileModalVisible = true;
  }

  openImportProxiesFromRemoteModal() {
    // Cannot import without choosing a group first
    if (this.selectedProxyGroup.id === -1) {
      this.messageService.add({
        severity: 'error',
        summary: 'No proxy group selected',
        detail: 'Select a valid proxy group, or create one, before importing proxies',
      });
      return;
    }

    this.importProxiesFromRemoteComponent?.reset();
    this.importProxiesFromRemoteModalVisible = true;
  }

  createProxyGroup(proxyGroup: CreateProxyGroupDto) {
    this.proxyGroupService.createProxyGroup(proxyGroup).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Created',
        detail: `Proxy group ${resp.name} was created`,
      });
      this.createProxyGroupModalVisible = false;
      this.refreshProxyGroups(resp.id);
    });
  }

  updateProxyGroup(proxyGroup: UpdateProxyGroupDto) {
    this.proxyGroupService.updateProxyGroup(proxyGroup).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Updated',
        detail: `Proxy group ${resp.name} was updated`,
      });
      this.updateProxyGroupModalVisible = false;
      this.refreshProxyGroups(resp.id);
    });
  }

  deleteProxyGroup(proxyGroup: ProxyGroupDto) {
    this.proxyGroupService.deleteProxyGroup(proxyGroup.id).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `Proxy group ${proxyGroup.name} was deleted`,
      });
      this.refreshProxyGroups();
    });
  }

  confirmDeleteProxyGroup() {
    this.confirmationService.confirm({
      message: `You are about to delete the proxy group 
      ${this.selectedProxyGroup.name},
      including all the proxies that it contains. 
      Are you sure that you want to proceed?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteProxyGroup(this.selectedProxyGroup),
    });
  }

  deleteProxies(proxyGroup: ProxyGroupDto, filters: ProxyFiltersDto) {
    this.proxyService.deleteProxies(filters).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `${resp.count} proxies were deleted from proxy group ${proxyGroup.name}`,
      });
      this.refreshProxies();
    });
  }

  deleteFilteredProxies() {
    this.deleteProxies(this.selectedProxyGroup, {
      pageNumber: null,
      pageSize: null,
      proxyGroupId: this.selectedProxyGroup.id,
      searchTerm: this.searchTerm,
      type: this.proxyType === 'anyType' ? null : this.proxyType,
      status: this.proxyWorkingStatus === 'anyStatus' ? null : this.proxyWorkingStatus,
      sortBy: null,
      sortDescending: false,
    });
  }

  deleteNotWorkingProxies() {
    this.deleteProxies(this.selectedProxyGroup, {
      pageNumber: null,
      pageSize: null,
      proxyGroupId: this.selectedProxyGroup.id,
      searchTerm: null,
      type: null,
      status: ProxyWorkingStatus.NotWorking,
      sortBy: null,
      sortDescending: false,
    });
  }

  deleteUntestedProxies() {
    this.deleteProxies(this.selectedProxyGroup, {
      pageNumber: null,
      pageSize: null,
      proxyGroupId: this.selectedProxyGroup.id,
      searchTerm: null,
      type: null,
      status: ProxyWorkingStatus.Untested,
      sortBy: null,
      sortDescending: false,
    });
  }

  openDeleteSlowProxiesModal() {
    this.deleteSlowProxiesModalVisible = true;
  }

  deleteSlowProxies(params: DeleteSlowProxiesParams) {
    this.proxyService.deleteSlowProxies(params.proxyGroupId, params.maxPing).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `${resp.count} proxies were deleted from proxy group ${this.selectedProxyGroup.name}`,
      });
      this.deleteSlowProxiesModalVisible = false;
      this.refreshProxies();
    });
  }

  confirmDeleteAllProxies() {
    this.confirmationService.confirm({
      message: `You are about to delete all proxies from proxy group 
      ${this.selectedProxyGroup.name}.
      Are you sure that you want to proceed?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () =>
        this.deleteProxies(this.selectedProxyGroup, {
          pageNumber: null,
          pageSize: null,
          proxyGroupId: this.selectedProxyGroup.id,
          searchTerm: null,
          type: null,
          status: null,
          sortBy: null,
          sortDescending: false,
        }),
    });
  }

  getWorkingClass(status: string) {
    if (status === 'working') {
      return 'color-good';
    }

    if (status === 'notWorking') {
      return 'color-bad';
    }

    return 'color-inactive';
  }

  importProxiesFromText(toImport: ProxiesToImport) {
    this.proxyService
      .addProxiesFromList({
        defaultUsername: toImport.defaultUsername,
        defaultPassword: toImport.defaultPassword,
        defaultType: toImport.defaultType,
        proxyGroupId: this.selectedProxyGroup.id,
        proxies: toImport.proxies,
      })
      .subscribe((resp) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Imported',
          detail: `${resp.count} proxies were imported to proxy group ${this.selectedProxyGroup.name}`,
        });

        // This function can be invoked from both the text and file import
        // so we need to close both of them
        this.importProxiesFromTextModalVisible = false;
        this.importProxiesFromFileModalVisible = false;
        this.refreshProxies();
      });
  }

  importProxiesFromRemote(toImport: RemoteProxiesToImport) {
    this.proxyService
      .addProxiesFromRemote({
        defaultUsername: toImport.defaultUsername,
        defaultPassword: toImport.defaultPassword,
        defaultType: toImport.defaultType,
        proxyGroupId: this.selectedProxyGroup.id,
        url: toImport.url,
      })
      .subscribe((resp) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Imported',
          detail: `${resp.count} proxies were imported to proxy group ${this.selectedProxyGroup.name}`,
        });
        this.importProxiesFromRemoteModalVisible = false;
        this.refreshProxies();
      });
  }

  searchBoxKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      this.refreshProxies();
    }
  }

  moveProxies(destinationProxyGroup: ProxyGroupDto) {
    this.proxyService
      .moveProxies({
        pageNumber: null,
        pageSize: null,
        proxyGroupId: this.selectedProxyGroup.id,
        destinationGroupId: destinationProxyGroup.id,
        searchTerm: this.searchTerm,
        type: this.proxyType === 'anyType' ? null : this.proxyType,
        status: this.proxyWorkingStatus === 'anyStatus' ? null : this.proxyWorkingStatus,
      })
      .subscribe((resp) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Moved',
          detail: `${resp.count} proxies were moved from proxy group ${this.selectedProxyGroup.name} to proxy group ${destinationProxyGroup.name}`,
        });
        this.refreshProxies();
      });
  }

  downloadProxies() {
    this.proxyService
      .downloadProxies({
        pageNumber: null,
        pageSize: null,
        proxyGroupId: this.selectedProxyGroup.id,
        searchTerm: this.searchTerm,
        type: this.proxyType === 'anyType' ? null : this.proxyType,
        status: this.proxyWorkingStatus === 'anyStatus' ? null : this.proxyWorkingStatus,
        sortBy: null,
        sortDescending: false,
      })
      .subscribe((resp) => saveFile(resp));
  }
}
