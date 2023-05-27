import { Component, OnInit } from '@angular/core';
import { ProxyGroupDto } from '../../dtos/proxy-group/proxy-group.dto';
import { ProxyDto } from '../../dtos/proxy/proxy.dto';
import { faFilterCircleXmark, faKey, faPen, faPlus, faX } from '@fortawesome/free-solid-svg-icons';
import { ProxyGroupService } from '../../services/proxy-group.service';
import { ProxyService } from '../../services/proxy.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PagedList } from '../../dtos/common/paged-list.dto';
import { UpdateProxyGroupDto } from '../../dtos/proxy-group/update-proxy-group.dto';
import { CreateProxyGroupDto } from '../../dtos/proxy-group/create-proxy-group.dto';

@Component({
  selector: 'app-proxies',
  templateUrl: './proxies.component.html',
  styleUrls: ['./proxies.component.scss']
})
export class ProxiesComponent implements OnInit {
  proxyGroups: ProxyGroupDto[] | null = null;
  proxies: PagedList<ProxyDto> | null = null;

  faPen = faPen;
  faKey = faKey;
  faFilterCircleXmark = faFilterCircleXmark;
  faX = faX;
  faPlus = faPlus;

  defaultProxyGroup = {
    id: -1,
    name: 'All',
    owner: { id: -2, username: 'System' }
  };

  selectedProxyGroup: ProxyGroupDto = this.defaultProxyGroup;

  createProxyGroupModalVisible = false;
  updateProxyGroupModalVisible = false;
  importProxiesModalVisible = false;

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

  refreshProxies() {
    this.proxyService.getProxies({
      pageNumber: 1,
      pageSize: 5,
      proxyGroupId: this.selectedProxyGroup.id,
      searchTerm: null,
      type: null,
      status: null
    }).subscribe(proxies => this.proxies = proxies);
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
}
