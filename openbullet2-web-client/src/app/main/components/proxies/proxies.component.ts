import { Component, OnInit } from '@angular/core';
import { ProxyGroupDto } from '../../dtos/proxy-group/proxy-group.dto';
import { ProxyDto } from '../../dtos/proxy/proxy.dto';
import { faEye, faEyeSlash, faFilterCircleXmark, faKey, faPen, faPlus, faX } from '@fortawesome/free-solid-svg-icons';
import { ProxyGroupService } from '../../services/proxy-group.service';
import { ProxyService } from '../../services/proxy.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PagedList } from '../../dtos/common/paged-list.dto';
import { UpdateProxyGroupDto } from '../../dtos/proxy-group/update-proxy-group.dto';
import { CreateProxyGroupDto } from '../../dtos/proxy-group/create-proxy-group.dto';
import { MenuItem } from 'primeng/api';
import { ProxyFiltersDto } from '../../dtos/proxy/proxy-filter.dto';
import { DeleteSlowProxiesParams } from './delete-slow-proxies/delete-slow-proxies.component';

@Component({
  selector: 'app-proxies',
  templateUrl: './proxies.component.html',
  styleUrls: ['./proxies.component.scss']
})
export class ProxiesComponent implements OnInit {
  proxyGroups: ProxyGroupDto[] | null = null;
  proxies: PagedList<ProxyDto> | null = null;
  rowCount: number = 10;

  faPen = faPen;
  faKey = faKey;
  faFilterCircleXmark = faFilterCircleXmark;
  faX = faX;
  faPlus = faPlus;
  faEye = faEye;
  faEyeSlash = faEyeSlash;

  defaultProxyGroup = {
    id: -1,
    name: 'All',
    owner: { id: -2, username: 'System' }
  };

  selectedProxyGroup: ProxyGroupDto = this.defaultProxyGroup;
  showPasswords: boolean = false;

  createProxyGroupModalVisible = false;
  updateProxyGroupModalVisible = false;
  importProxiesModalVisible = false;
  deleteSlowProxiesModalVisible = false;

  searchTerm: string = '';
  proxyWorkingStatus: string = 'anyStatus';
  proxyWorkingStatuses: string[] = [
    'anyStatus',
    'working',
    'notWorking',
    'untested'
  ];
  proxyType: string = 'anyType';
  proxyTypes: string[] = [
    'anyType',
    'http',
    'socks4',
    'socks5',
    'socks4a'
  ];

  proxyMenuItems: MenuItem[] = [
    {
      label: 'Import',
      icon: 'pi pi-fw pi-file-import',
      command: e => console.log(e)
    },
    {
      label: 'Export',
      icon: 'pi pi-fw pi-file-export',
      command: e => console.log(e)
    },
    {
      label: 'Edit',
      icon: 'pi pi-fw pi-pencil',
      items: [
        {
          label: 'Filtered proxies',
          icon: 'pi pi-fw pi-filter-fill',
          items: [
            {
              label: 'Move',
              icon: 'pi pi-fw pi-arrow-right',
              command: e => console.log(e)
            },
            {
              label: 'Delete',
              icon: 'pi pi-fw pi-trash color-bad',
              command: e => this.deleteFilteredProxies()
            }
          ]
        }
      ]
    },
    {
      label: 'Delete',
      icon: 'pi pi-fw pi-trash',
      items: [
        {
          label: 'All proxies',
          icon: 'pi pi-fw pi-trash color-bad',
          command: e => this.confirmDeleteAllProxies()
        },
        {
          label: 'Not working proxies',
          icon: 'pi pi-fw pi-trash color-bad',
          command: e => this.deleteNotWorkingProxies()
        },
        {
          label: 'Untested proxies',
          icon: 'pi pi-fw pi-trash color-bad',
          command: e => this.deleteUntestedProxies()
        },
        {
          label: 'Slow proxies',
          icon: 'pi pi-fw pi-trash color-bad',
          command: e => this.openDeleteSlowProxiesModal()
        }
      ]
    }
  ];

  constructor(private proxyGroupService: ProxyGroupService,
    private proxyService: ProxyService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService) {

  }

  ngOnInit(): void {
    this.refreshProxyGroups();
  }

  refreshProxyGroups() {
    this.proxyGroupService.getAllProxyGroups()
      .subscribe(proxyGroups => {
        this.proxies = null;
        this.selectedProxyGroup = this.defaultProxyGroup;
        this.proxyGroups = [
          this.defaultProxyGroup,
          ...proxyGroups
        ];
        this.refreshProxies();
      });
  }

  // TODO: Only call this when necessary, don't make double calls!
  refreshProxies(pageNumber: number = 1, pageSize: number | null = null) {
    this.proxyService.getProxies({
      pageNumber,
      pageSize: pageSize ?? this.rowCount,
      proxyGroupId: this.selectedProxyGroup.id,
      searchTerm: this.searchTerm,
      type: this.proxyType === 'anyType' ? null : this.proxyType,
      status: this.proxyWorkingStatus === 'anyStatus' ? null : this.proxyWorkingStatus
    }).subscribe(proxies => this.proxies = proxies);
  }

  lazyLoadProxies(event: any) {
    this.refreshProxies(
      Math.floor(event.first / event.rows) + 1,
      event.rows
    );
  }

  proxyGroupSelected() {
    this.refreshProxies();
  }

  openCreateProxyGroupModal() {
    this.createProxyGroupModalVisible = true;
  }

  openUpdateProxyGroupModal() {
    this.updateProxyGroupModalVisible = true;
  }

  openImportProxiesModal() {
    this.importProxiesModalVisible = true;
  }

  createProxyGroup(proxyGroup: CreateProxyGroupDto) {
    this.proxyGroupService.createProxyGroup(proxyGroup)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Created',
          detail: `Proxy group ${resp.name} was created`
        });
        this.createProxyGroupModalVisible = false;
        this.refreshProxyGroups();
      });
  }

  updateProxyGroup(proxyGroup: UpdateProxyGroupDto) {
    this.proxyGroupService.updateProxyGroup(proxyGroup)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Updated',
          detail: `Proxy group ${resp.name} was updated`
        });
        this.updateProxyGroupModalVisible = false;
        this.refreshProxyGroups();
      });
  }

  deleteProxyGroup(proxyGroup: ProxyGroupDto) {
    this.proxyGroupService.deleteProxyGroup(proxyGroup.id)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Deleted',
          detail: `Proxy group ${proxyGroup.name} was deleted`
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
      accept: () => this.deleteProxyGroup(this.selectedProxyGroup)
    });
  }

  deleteProxies(proxyGroup: ProxyGroupDto, filters: ProxyFiltersDto) {
    this.proxyService.deleteProxies(filters).subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Deleted',
          detail: `${resp.count} proxies were deleted from proxy group ${proxyGroup.name}`
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
      status: this.proxyWorkingStatus === 'anyStatus' ? null : this.proxyWorkingStatus
    });
  }

  deleteNotWorkingProxies() {
    this.deleteProxies(this.selectedProxyGroup, {
      pageNumber: null,
      pageSize: null,
      proxyGroupId: this.selectedProxyGroup.id,
      searchTerm: null,
      type: null,
      status: 'notWorking'
    });
  }

  deleteUntestedProxies() {
    this.deleteProxies(this.selectedProxyGroup, {
      pageNumber: null,
      pageSize: null,
      proxyGroupId: this.selectedProxyGroup.id,
      searchTerm: null,
      type: null,
      status: 'untested'
    });
  }

  openDeleteSlowProxiesModal() {
    this.deleteSlowProxiesModalVisible = true;
  }

  deleteSlowProxies(params: DeleteSlowProxiesParams) {
    this.proxyService.deleteSlowProxies(params.proxyGroupId, params.maxPing)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Deleted',
          detail: `${resp.count} proxies were deleted from proxy group ${this.selectedProxyGroup.name}`
        });
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
      accept: () => this.deleteProxies(this.selectedProxyGroup, {
        pageNumber: null,
        pageSize: null,
        proxyGroupId: this.selectedProxyGroup.id,
        searchTerm: null,
        type: null,
        status: null
      })
    });
  }

  getWorkingClass(status: string) {
    if (status === 'working') {
      return 'color-good';
    } else if (status === 'notWorking') {
      return 'color-bad';
    } else {
      return 'color-inactive';
    }
  }
}
